using System;
using System.Collections;
using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using _01.Scripts._11.HabitatMode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// StageManager의 Awake 이후, 전투 오브젝트의 Start 이전에 스테이지를 적용한다.
[DefaultExecutionOrder(100)]
public class TutorialManager : MonoBehaviour
{
    public enum WaitType { Swap, NextClick, Delay, Hold, SpecialTile }
    public enum PanelType { Top, Under }
    public enum StepAction { None, CreateObstacle }

    [Serializable]
    public class TutorialStep
    {
        [Min(0)] public int dialogueIndex;
        public WaitType waitType;
        public PanelType panel;
        public Vector2Int from;
        public Vector2Int to;
        public SpecialPuzzleType specialType = SpecialPuzzleType.RowBomb;
        public StepAction enterAction;
        public Vector2Int obstaclePosition;
        public ObstaclePuzzleType obstacleType;
        [Min(0f)] public float delay = 1f;
    }

    [Header("Tutorial Stage")]
    [Tooltip("내부 스테이지 번호입니다. 0 = 화면의 1스테이지(최저 난이도).")]
    [Min(0)] [SerializeField] private int tutorialStage = 0;

    [SerializeField] private CanvasGroup timeInfo;
    [SerializeField] private CanvasGroup lockButton;
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private CanvasGroup underTutorialPanel;
    [SerializeField] private TutorialText tutorialTextList;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TextMeshProUGUI underTutorialText;
    [SerializeField] private PuzzleGenerator puzzleGenerator;
    [Tooltip("미지정 시 상단 대사 패널 전체를 다음 버튼으로 사용합니다.")]
    [SerializeField] private Button nextButton;
    [Tooltip("미지정 시 하단 대사 패널 전체를 다음 버튼으로 사용합니다.")]
    [SerializeField] private Button underNextButton;

    [SerializeField] private TutorialStep[] steps =
    {
        new TutorialStep
        {
            dialogueIndex = 0, waitType = WaitType.Swap,
            from = new Vector2Int(3, 4), to = new Vector2Int(3, 3)
        },
        new TutorialStep { dialogueIndex = 1, waitType = WaitType.NextClick },
        new TutorialStep { dialogueIndex = 2, waitType = WaitType.NextClick },
        new TutorialStep { dialogueIndex = 3, waitType = WaitType.NextClick, panel = PanelType.Under },
        new TutorialStep { dialogueIndex = 4, waitType = WaitType.NextClick, panel = PanelType.Under },
        new TutorialStep
        {
            dialogueIndex = 5, waitType = WaitType.Swap,
            from = new Vector2Int(2, 3), to = new Vector2Int(2, 2)
        },
        new TutorialStep
        {
            dialogueIndex = 6, waitType = WaitType.SpecialTile,
            from = new Vector2Int(2, 2), specialType = SpecialPuzzleType.RowBomb
        },
        new TutorialStep
        {
            dialogueIndex = 7, waitType = WaitType.Swap,
            from = new Vector2Int(3, 4), to = new Vector2Int(3, 3)
        },
        new TutorialStep
        {
            dialogueIndex = 8, waitType = WaitType.SpecialTile,
            from = new Vector2Int(3, 3), specialType = SpecialPuzzleType.CircleBomb
        },
        new TutorialStep { dialogueIndex = 9, waitType = WaitType.NextClick },
        new TutorialStep
        {
            dialogueIndex = 10, waitType = WaitType.Delay, delay = 0f,
            enterAction = StepAction.CreateObstacle,
            obstaclePosition = new Vector2Int(4, 2), obstacleType = ObstaclePuzzleType.Fixed
        },
        new TutorialStep { dialogueIndex = 11, waitType = WaitType.NextClick },
        new TutorialStep
        {
            dialogueIndex = 12, waitType = WaitType.Swap,
            from = new Vector2Int(3, 1), to = new Vector2Int(3, 2)
        },
        new TutorialStep
        {
            dialogueIndex = 13, waitType = WaitType.Swap,
            enterAction = StepAction.CreateObstacle,
            obstaclePosition = new Vector2Int(3, 3), obstacleType = ObstaclePuzzleType.DeActivated,
            from = new Vector2Int(2, 1), to = new Vector2Int(2, 2)
        },
        new TutorialStep { dialogueIndex = 14, waitType = WaitType.Hold }
    };

    private TutorialStep currentStep;
    private bool actionCompleted;
    private bool nextClicked;
    private bool ownsInputRestriction;
    private StageManager stageManager;
    private HabitatModeManager habitatModeManager;
    private int previousStage;
    private bool previousHabitatBattle;
    private bool stageApplied;

    private void Awake()
    {
        if (!GameManager.Instance.Istutorial)
            return;

        stageManager = StageManager.Instance;
        previousStage = stageManager.CurrentStage;
        stageManager.SetStage(Mathf.Max(0, tutorialStage));
        habitatModeManager = HabitatModeManager.Instance;
        if (habitatModeManager != null)
        {
            previousHabitatBattle = habitatModeManager.IsHabitatBattle;
            habitatModeManager.IsHabitatBattle = false;
        }
        stageApplied = true;

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

        nextButton = ConfigureNextButton(tutorialPanel, nextButton);
        if (underTutorialPanel != null)
            underNextButton = ConfigureNextButton(underTutorialPanel, underNextButton);
        puzzleGenerator.OnSuccessfulSwapResolved += HandleSuccessfulSwap;
        puzzleGenerator.OnSpecialTileResolved += HandleSpecialTile;
        SetUi(timeInfo, 0f);
        SetUi(lockButton, 0f);
        SetUi(underTutorialPanel, 0f);

        foreach (TutorialStep step in steps)
        {
            currentStep = step;
            actionCompleted = false;
            nextClicked = false;
            puzzleGenerator.SetTutorialInputRestriction(true);
            ShowStep(step);

            if (step.enterAction == StepAction.CreateObstacle)
            {
                yield return new WaitUntil(() => puzzleGenerator.IsBoardReady);
                if (!puzzleGenerator.TryCreateTutorialObstacle(
                        step.obstaclePosition, step.obstacleType))
                    yield break;
                yield return new WaitUntil(() => puzzleGenerator.IsBoardReady);
            }

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
                case WaitType.SpecialTile:
                    yield return new WaitUntil(() => puzzleGenerator.IsBoardReady);
                    puzzleGenerator.SetTutorialInputRestriction(true, specialAllowed: IsExpectedSpecialTile);
                    yield return new WaitUntil(() => actionCompleted);
                    yield return new WaitUntil(() => puzzleGenerator.IsBoardReady);
                    break;
                case WaitType.Delay:
                    yield return new WaitForSecondsRealtime(step.delay);
                    break;
                case WaitType.Hold:
                    // 후속 단계가 준비될 때까지 현재 대사를 유지한다.
                    yield return new WaitUntil(() => false);
                    break;
            }
        }

        currentStep = null;
        SetPanel(tutorialPanel, false, false);
        SetPanel(underTutorialPanel, false, false);
        ReleaseInputRestriction();
    }

    private Button ConfigureNextButton(CanvasGroup panel, Button button)
    {
        if (button == null)
        {
            button = panel.GetComponent<Button>();
            if (button == null)
            {
                button = panel.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.targetGraphic = panel.GetComponent<Graphic>();
            }
        }
        button.onClick.AddListener(NextDialogue);
        return button;
    }

    private void ShowStep(TutorialStep step)
    {
        bool useUnderPanel = step.panel == PanelType.Under;
        bool canAdvance = step.waitType == WaitType.NextClick;
        TextMeshProUGUI text = useUnderPanel ? underTutorialText : tutorialText;
        text.text = tutorialTextList.GetDialogue(step.dialogueIndex);

        SetPanel(tutorialPanel, !useUnderPanel, canAdvance);
        SetPanel(underTutorialPanel, useUnderPanel, canAdvance);
        nextButton.interactable = !useUnderPanel && canAdvance;
        if (underNextButton != null)
            underNextButton.interactable = useUnderPanel && canAdvance;
    }

    private static void SetPanel(CanvasGroup panel, bool visible, bool interactable)
    {
        if (panel == null)
            return;

        SetUi(panel, visible ? 1f : 0f, visible && interactable);
        panel.gameObject.SetActive(visible);
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

    private bool IsExpectedSpecialTile(Vector2Int position, SpecialPuzzleType type)
    {
        return currentStep != null && currentStep.waitType == WaitType.SpecialTile &&
               !actionCompleted && position == currentStep.from && type == currentStep.specialType;
    }

    private void HandleSpecialTile(Vector2Int position, SpecialPuzzleType type)
    {
        if (!IsExpectedSpecialTile(position, type))
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
            if (step.panel == PanelType.Under && (underTutorialPanel == null || underTutorialText == null))
            {
                Debug.LogError("TutorialManager: 하단 대사 패널과 하단 대사 UI를 연결해 주세요.", this);
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
        if (underNextButton != null)
            underNextButton.onClick.RemoveListener(NextDialogue);
        if (puzzleGenerator != null)
        {
            puzzleGenerator.OnSuccessfulSwapResolved -= HandleSuccessfulSwap;
            puzzleGenerator.OnSpecialTileResolved -= HandleSpecialTile;
        }
        ReleaseInputRestriction();
    }

    private void OnDestroy()
    {
        if (!stageApplied)
            return;

        if (stageManager != null)
            stageManager.SetStage(previousStage);
        if (habitatModeManager != null)
            habitatModeManager.IsHabitatBattle = previousHabitatBattle;
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
