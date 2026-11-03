using System;
using System.Collections;
using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public enum WaitType { Swap, NextClick, Delay, Hold }

    [Serializable]
    public class TutorialStep
    {
        [Min(0)] public int dialogueIndex;
        public WaitType waitType;
        public Vector2Int from;
        public Vector2Int to;
        [Min(0f)] public float delay = 1f;
    }

    [SerializeField] private CanvasGroup timeInfo;
    [SerializeField] private CanvasGroup lockButton;
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private CanvasGroup underTutorialPanel;
    [SerializeField] private TutorialText tutorialTextList;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private PuzzleGenerator puzzleGenerator;
    [Tooltip("미지정 시 상단 대사 패널 전체를 다음 버튼으로 사용합니다.")]
    [SerializeField] private Button nextButton;

    [SerializeField] private TutorialStep[] steps =
    {
        new TutorialStep
        {
            dialogueIndex = 0, waitType = WaitType.Swap,
            from = new Vector2Int(3, 4), to = new Vector2Int(3, 3)
        },
        new TutorialStep { dialogueIndex = 1, waitType = WaitType.NextClick },
        new TutorialStep { dialogueIndex = 2, waitType = WaitType.Hold }
    };

    private TutorialStep currentStep;
    private bool actionCompleted;
    private bool nextClicked;
    private bool ownsInputRestriction;

    private void Awake()
    {
        if (!GameManager.Instance.Istutorial)
            return;

        if (puzzleGenerator == null)
            puzzleGenerator = FindFirstObjectByType<PuzzleGenerator>();

        // Start 순서와 관계없이 보드 생성 중의 입력도 차단한다.
        if (puzzleGenerator != null)
        {
            puzzleGenerator.SetTutorialInputRestriction(true);
            ownsInputRestriction = true;
        }
    }

    private IEnumerator Start()
    {
        if (!GameManager.Instance.Istutorial)
            yield break;

        if (!ValidateSetup())
        {
            ReleaseInputRestriction();
            yield break;
        }

        if (nextButton == null)
        {
            nextButton = tutorialPanel.GetComponent<Button>();
            if (nextButton == null)
            {
                nextButton = tutorialPanel.gameObject.AddComponent<Button>();
                nextButton.transition = Selectable.Transition.None;
                nextButton.targetGraphic = tutorialPanel.GetComponent<Graphic>();
            }
        }

        nextButton.onClick.AddListener(NextDialogue);
        puzzleGenerator.OnSuccessfulSwapResolved += HandleSuccessfulSwap;
        SetUi(timeInfo, 0f);
        SetUi(lockButton, 0f);
        SetUi(underTutorialPanel, 0f);

        foreach (TutorialStep step in steps)
        {
            currentStep = step;
            actionCompleted = false;
            nextClicked = false;
            puzzleGenerator.SetTutorialInputRestriction(true);
            tutorialText.text = tutorialTextList.GetDialogue(step.dialogueIndex);
            SetUi(tutorialPanel, 1f, step.waitType == WaitType.NextClick);
            nextButton.interactable = step.waitType == WaitType.NextClick;

            switch (step.waitType)
            {
                case WaitType.Swap:
                    yield return new WaitUntil(() => puzzleGenerator.IsBoardReady);
                    puzzleGenerator.SetTutorialInputRestriction(true, IsExpectedSwap);
                    yield return new WaitUntil(() => actionCompleted);
                    yield return new WaitUntil(() => puzzleGenerator.IsBoardReady);
                    break;
                case WaitType.NextClick:
                    yield return new WaitUntil(() => nextClicked);
                    break;
                case WaitType.Delay:
                    yield return new WaitForSecondsRealtime(step.delay);
                    break;
                case WaitType.Hold:
                    // 이번 구현은 text02를 유지한다. 후속 단계 추가 시 대기 유형을 변경한다.
                    yield return new WaitUntil(() => false);
                    break;
            }
        }

        currentStep = null;
        SetUi(tutorialPanel, 0f);
        ReleaseInputRestriction();
    }

    public void NextDialogue()
    {
        if (currentStep != null && currentStep.waitType == WaitType.NextClick)
            nextClicked = true;
    }

    private bool IsExpectedSwap(Vector2Int from, Vector2Int to)
    {
        return currentStep != null && currentStep.waitType == WaitType.Swap &&
               !actionCompleted &&
               ((from == currentStep.from && to == currentStep.to) ||
                (from == currentStep.to && to == currentStep.from));
    }

    private void HandleSuccessfulSwap(Vector2Int from, Vector2Int to)
    {
        if (!IsExpectedSwap(from, to))
            return;

        actionCompleted = true;
        puzzleGenerator.SetTutorialInputRestriction(true);
    }

    private bool ValidateSetup()
    {
        if (puzzleGenerator == null || tutorialTextList == null ||
            tutorialText == null || tutorialPanel == null || steps == null || steps.Length == 0)
        {
            Debug.LogError("TutorialManager: 퍼즐, 대사 목록, 대사 UI, 패널 및 단계를 확인해 주세요.", this);
            return false;
        }

        foreach (TutorialStep step in steps)
        {
            if (step == null || step.dialogueIndex < 0 || step.dialogueIndex >= tutorialTextList.Count)
            {
                Debug.LogError("TutorialManager: 단계의 대사 인덱스가 유효하지 않습니다.", this);
                return false;
            }
        }
        return true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        currentStep = null;
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextDialogue);
        if (puzzleGenerator != null)
            puzzleGenerator.OnSuccessfulSwapResolved -= HandleSuccessfulSwap;
        ReleaseInputRestriction();
    }

    private void ReleaseInputRestriction()
    {
        if (ownsInputRestriction && puzzleGenerator != null)
            puzzleGenerator.SetTutorialInputRestriction(false);
        ownsInputRestriction = false;
    }

    private static void SetUi(CanvasGroup group, float alpha, bool interactable = false)
    {
        if (group == null)
            return;

        group.alpha = alpha;
        group.interactable = interactable;
        group.blocksRaycasts = alpha > 0f && interactable;
    }
}
