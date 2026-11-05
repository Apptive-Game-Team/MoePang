using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using _01.Scripts._02.Unit;
using _01.Scripts._11.HabitatMode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [Header("Tutorial Enemy Stats")]
    [Min(1f)] [SerializeField] private float tutorialEnemyMaxHp = 2f;
    [Min(0f)] [SerializeField] private float tutorialEnemyAttackDamage = 0f;

    [Header("대사 진행")]
    [Tooltip("대사 표시 후 다음 이벤트까지 기다리는 시간입니다. 화면을 누르면 즉시 진행합니다.")]
    [Min(0f)] [SerializeField] private float dialogueDuration = 5f;

    [Tooltip("퍼즐 처리와 모든 소환이 끝난 뒤 다음 대사까지 기다리는 시간입니다.")]
    [Min(0f)] [SerializeField] private float postActionDelay = 1f;

    private static TutorialManager activeTutorial;
    // 0: 기존 소환 규칙, 1: 햇빛 이전, 2: 햇빛 사용으로 발생하는 소환.
    private int friendlyGradeOverride = 1;

    public static int FriendlyGradeOverride =>
        activeTutorial != null && activeTutorial.isActiveAndEnabled && GameManager.Instance.Istutorial
            ? activeTutorial.friendlyGradeOverride : 0;

    public static bool UseUnscaledPuzzleTime => activeTutorial != null &&
        activeTutorial.isActiveAndEnabled && activeTutorial.ownsTimeScale &&
        activeTutorial.usesUnscaledPuzzleTime;

    public static bool IsTutorialPopupOpen => UseUnscaledPuzzleTime && activeTutorial.endPopupOpen;

    public static bool TryGetFriendlyUnitOverride(
        IReadOnlyList<FriendlyUnitData> candidates, out FriendlyUnitData unit, int grade)
    {
        unit = null;
        if (activeTutorial == null || !activeTutorial.isActiveAndEnabled ||
            !GameManager.Instance.Istutorial || grade == 0)
            return false;

        if (candidates != null)
        {
            foreach (FriendlyUnitData candidate in candidates)
            {
                if (candidate != null && candidate.UnitGrade == grade)
                {
                    unit = candidate;
                    return true;
                }
            }
        }

        // 요청한 등급이 없다면 다른 등급으로 대체하지 않는다.
        Debug.LogError($"TutorialManager: 해당 서식지에 {grade}단계 기물이 없습니다.", activeTutorial);
        return true;
    }

    public static bool TryGetEnemyStats(out float maxHp, out float attackDamage)
    {
        maxHp = 0f;
        attackDamage = 0f;
        if (activeTutorial == null || !activeTutorial.isActiveAndEnabled ||
            !GameManager.Instance.Istutorial)
            return false;

        maxHp = Mathf.Max(1f, activeTutorial.tutorialEnemyMaxHp);
        attackDamage = Mathf.Max(0f, activeTutorial.tutorialEnemyAttackDamage);
        return true;
    }

    [SerializeField] private CanvasGroup timeInfo;
    [SerializeField] private CanvasGroup lockButton;
    [SerializeField] private CanvasGroup pauseButton;
    [SerializeField] private CanvasGroup tutorialPanel;
    [SerializeField] private CanvasGroup underTutorialPanel;
    [SerializeField] private TutorialText tutorialTextList;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TextMeshProUGUI underTutorialText;
    [SerializeField] private PuzzleGenerator puzzleGenerator;
    [SerializeField] private GameObject tutorialEndButton;
    [SerializeField] private GameObject tutorialEndPopup;
    // 기존 씬의 참조는 보존하되, 대사 진행 입력은 화면 전체 영역에서 받습니다.
    [SerializeField, HideInInspector] private Button nextButton;
    [SerializeField, HideInInspector] private Button underNextButton;

    [Header("첫 단계 하이라이트")]
    [SerializeField] private TutorialHighlightMask highlightMask;
    [Tooltip("0부터 시작하는 보드 좌표입니다. 필요한 만큼 칸을 추가할 수 있습니다.")]
    [SerializeField] private Vector2Int[] firstStepHighlightCells =
    {
        new Vector2Int(3, 4),
        new Vector2Int(2, 3),
        new Vector2Int(3, 3),
        new Vector2Int(4, 3)
    };

    [Header("첫 교환 원형 힌트")]
    [Tooltip("첫 단계에서 매치 줄 밖의 숲 타일 (3,4) 중앙에 표시할 UI 프리팹입니다.")]
    [SerializeField] private GameObject hintCirclePrefab;

    [Header("교환 타일 힌트 애니메이션")]
    [Range(0f, 1f)] [SerializeField] private float swapHintBrightness = 0.6f;
    [Range(0f, 0.3f)] [SerializeField] private float swapHintScaleAmount = 0.08f;
    [Min(0.05f)] [SerializeField] private float swapHintPulseDuration = 0.25f;
    [Min(0f)] [SerializeField] private float swapHintPulseGap = 0.12f;
    [Min(0f)] [SerializeField] private float swapHintRepeatDelay = 1f;
    [Tooltip("타일이 밝아질 때 함께 표시하는 외곽 글로우 색입니다. HDR 색상과 알파로 강도를 조절합니다.")]
    [ColorUsage(true, true)] [SerializeField] private Color swapHintGlowColor = new Color(5f, 4.5f, 2.5f, 0.65f);

    [Header("5단계 하이라이트")]
    [Tooltip("교환하는 두 칸과 교환 후 극지방 4매치가 되는 칸입니다. 보드 좌표는 0부터 시작합니다.")]
    [SerializeField] private Vector2Int[] step5HighlightCells =
    {
        new Vector2Int(2, 3),
        new Vector2Int(1, 2),
        new Vector2Int(2, 2),
        new Vector2Int(3, 2),
        new Vector2Int(4, 2)
    };

    [Header("7단계 하이라이트")]
    [Tooltip("교환하는 두 칸과 교환 후 극지방 5매치가 되는 칸입니다. 보드 좌표는 0부터 시작합니다.")]
    [SerializeField] private Vector2Int[] step7HighlightCells =
    {
        new Vector2Int(3, 4),
        new Vector2Int(1, 3),
        new Vector2Int(2, 3),
        new Vector2Int(3, 3),
        new Vector2Int(4, 3),
        new Vector2Int(5, 3)
    };

    [Header("12단계 하이라이트")]
    [Tooltip("교환하는 두 칸과 자물쇠 타일을 포함한 숲 3매치 칸입니다. 보드 좌표는 0부터 시작합니다.")]
    [SerializeField] private Vector2Int[] step12HighlightCells =
    {
        new Vector2Int(3, 1),
        new Vector2Int(2, 2),
        new Vector2Int(3, 2),
        new Vector2Int(4, 2)
    };

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
    private bool waitingForDialogue;
    private bool waitingForPuzzleAction;
    private bool ownsTimeScale;
    private bool usesUnscaledPuzzleTime;
    private bool puzzleActionRunning;
    private UnitSpawner friendlySpawner;
    private bool tutorialPaused;
    private float originalTimeScale;
    private float gameplayTimeScale;
    private bool ownsInputRestriction;
    private StageManager stageManager;
    private HabitatModeManager habitatModeManager;
    private int previousStage;
    private bool previousHabitatBattle;
    private bool stageApplied;
    private Button endButton;
    private Button endAcceptButton;
    private Button endCloseButton;
    private GameObject previousSelection;
    private bool endPopupOpen;
    private float previousTimeScale;
    private TutorialScreenNextInput screenNextInput;
    private TutorialSwapHintAnimation swapHint;
    private PuzzleObject hintPressedTile;
    private RectTransform hintCircleInstance;

    private void Awake()
    {
        if (highlightMask == null)
            highlightMask = GetComponent<TutorialHighlightMask>();

        if (!GameManager.Instance.Istutorial)
            return;

        activeTutorial = this;
        swapHint = new TutorialSwapHintAnimation();
        friendlyGradeOverride = 1;
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
        if (highlightMask != null)
            highlightMask.HideHighlights();

        if (tutorialEndPopup != null)
            tutorialEndPopup.SetActive(false);
        if (tutorialEndButton != null)
            tutorialEndButton.SetActive(GameManager.Instance.Istutorial);

        if (!GameManager.Instance.Istutorial)
            yield break;

        ConfigureEndPopup();
        if (!ValidateSetup())
        {
            ReleaseInputRestriction();
            yield break;
        }

        if (!ConfigureScreenNextInput())
        {
            ReleaseInputRestriction();
            yield break;
        }
        puzzleGenerator.OnSwapStarted += HandleSwapStarted;
        puzzleGenerator.OnPuzzlePointerDown += HandlePuzzlePointerDown;
        puzzleGenerator.OnPuzzlePointerUp += HandlePuzzlePointerUp;
        puzzleGenerator.OnSuccessfulSwapResolved += HandleSuccessfulSwap;
        puzzleGenerator.OnSpecialTileResolved += HandleSpecialTile;
        puzzleGenerator.OnSpecialTileStarted += HandleSpecialTileStarted;
        friendlySpawner = puzzleGenerator.FriendlySpawner;
        if (friendlySpawner != null)
            friendlySpawner.OnFriendlySpawned += HandleFriendlySpawned;
        SetUi(timeInfo, 0f);
        SetUi(lockButton, 0f);
        SetUi(pauseButton, 0f);
        SetPanel(tutorialPanel, false, false);
        SetPanel(underTutorialPanel, false, false);

        originalTimeScale = Time.timeScale;
        gameplayTimeScale = originalTimeScale > 0f ? originalTimeScale : 1f;
        ownsTimeScale = true;
        usesUnscaledPuzzleTime = true;
        // 초기 보드의 낙하가 끝난 뒤 첫 대사를 표시합니다.
        SetTutorialPaused(false);
        yield return new WaitUntil(CanContinueTutorial);

        foreach (TutorialStep step in steps)
        {
            swapHint?.Stop();
            DestroyHintCircle();
            hintPressedTile = null;
            SetTutorialPaused(true);
            currentStep = step;
            actionCompleted = false;
            puzzleActionRunning = false;
            waitingForPuzzleAction = false;
            nextClicked = false;
            puzzleGenerator.SetTutorialInputRestriction(true);
            waitingForDialogue = true;
            ShowStep(step);
            yield return WaitForDialogue();

            if (step.enterAction == StepAction.CreateObstacle)
            {
                yield return new WaitUntil(CanContinueTutorial);
                if (!puzzleGenerator.TryCreateTutorialObstacle(
                        step.obstaclePosition, step.obstacleType))
                {
                    ReleaseInputRestriction();
                    RestoreTutorialTimeScale();
                    yield break;
                }
                yield return new WaitUntil(CanContinueTutorial);
                SetTutorialPaused(true);
            }

            switch (step.waitType)
            {
                case WaitType.Swap:
                    yield return new WaitUntil(CanContinueTutorial);
                    Vector2Int[] highlightCells = GetSwapHighlightCells(step);
                    if (highlightCells != null)
                        ShowStepHighlights(step, highlightCells);
                    waitingForPuzzleAction = true;
                    puzzleGenerator.SetTutorialInputRestriction(true,
                        (from, to) => !endPopupOpen && waitingForPuzzleAction && IsExpectedSwap(from, to));
                    TryStartSwapHint();
                    yield return WaitForPuzzleResolution();
                    break;
                case WaitType.NextClick:
                    // 설명 단계는 공통 대사 대기가 끝나면 다음 대사로 진행합니다.
                    break;
                case WaitType.SpecialTile:
                    yield return new WaitUntil(CanContinueTutorial);
                    // 바람/햇빛 모두 해당 단계에서 사용할 특수 타일을 강조합니다.
                    ShowStepHighlights(step, new[] { step.from });
                    waitingForPuzzleAction = true;
                    puzzleGenerator.SetTutorialInputRestriction(true,
                        specialAllowed: (position, type) => !endPopupOpen && waitingForPuzzleAction &&
                                                           IsExpectedSpecialTile(position, type));
                    yield return WaitForPuzzleResolution();
                    break;
                case WaitType.Delay:
                    yield return WaitTutorialDelay(step.delay);
                    break;
                case WaitType.Hold:
                    // 마지막 안내 후에는 전투와 자유 퍼즐 조작을 계속할 수 있습니다.
                    usesUnscaledPuzzleTime = false;
                    SetTutorialPaused(false);
                    ReleaseInputRestriction();
                    yield return new WaitUntil(() => false);
                    break;
            }
        }

        currentStep = null;
        UpdateScreenNextInput();
        SetPanel(tutorialPanel, false, false);
        SetPanel(underTutorialPanel, false, false);
        ReleaseInputRestriction();
        RestoreTutorialTimeScale();
    }

    private bool CanContinueTutorial() => !endPopupOpen && puzzleGenerator.IsBoardReady;

    private void SetTutorialPaused(bool paused)
    {
        tutorialPaused = paused;
        ApplyTutorialTimeScale();
    }

    private void ApplyTutorialTimeScale()
    {
        if (ownsTimeScale)
            Time.timeScale = endPopupOpen || tutorialPaused ? 0f : gameplayTimeScale;
    }

    private void RestoreTutorialTimeScale()
    {
        if (!ownsTimeScale)
            return;
        ownsTimeScale = false;
        usesUnscaledPuzzleTime = false;
        Time.timeScale = originalTimeScale;
    }

    private IEnumerator WaitForPuzzleResolution()
    {
        yield return new WaitUntil(() => !endPopupOpen && actionCompleted && puzzleGenerator.IsBoardReady);
        if (highlightMask != null)
            highlightMask.HideHighlights();

        yield return new WaitUntil(() => !endPopupOpen && puzzleGenerator.IsTutorialResolutionComplete);
        yield return WaitTutorialDelay(postActionDelay);
        puzzleActionRunning = false;
        SetTutorialPaused(true);
    }

    private IEnumerator WaitForDialogue()
    {
        float elapsed = 0f;
        while (endPopupOpen || (!nextClicked && elapsed < dialogueDuration))
        {
            bool popupWasOpen = endPopupOpen;
            yield return null;
            // 종료 팝업을 보고 있던 시간은 대사 표시 시간에서 제외합니다.
            if (!popupWasOpen && !endPopupOpen)
                elapsed += Time.unscaledDeltaTime;
        }

        waitingForDialogue = false;
        UpdateScreenNextInput();
    }

    private IEnumerator WaitTutorialDelay(float duration)
    {
        float elapsed = 0f;
        while (endPopupOpen || elapsed < duration)
        {
            bool popupWasOpen = endPopupOpen;
            yield return null;
            if (!popupWasOpen && !endPopupOpen)
                elapsed += Time.unscaledDeltaTime;
        }
    }

    private void ConfigureEndPopup()
    {
        if (tutorialEndButton == null || tutorialEndPopup == null ||
            tutorialEndPopup.transform.childCount < 4)
        {
            Debug.LogError("TutorialManager: 종료 버튼과 팝업(자식 3: Accept, 4: Close)을 연결해 주세요.", this);
            return;
        }

        endButton = tutorialEndButton.GetComponent<Button>();
        endAcceptButton = tutorialEndPopup.transform.GetChild(2).GetComponent<Button>();
        endCloseButton = tutorialEndPopup.transform.GetChild(3).GetComponent<Button>();
        if (endButton == null || endAcceptButton == null || endCloseButton == null)
        {
            Debug.LogError("TutorialManager: 종료/확인/닫기 오브젝트에 Button이 필요합니다.", this);
            return;
        }

        // 복제한 상점 팝업의 기존 이벤트는 이 팝업에서만 교체한다.
        endAcceptButton.onClick = new Button.ButtonClickedEvent();
        endCloseButton.onClick = new Button.ButtonClickedEvent();
        endButton.onClick.AddListener(OpenEndPopup);
        endAcceptButton.onClick.AddListener(AcceptEndPopup);
        endCloseButton.onClick.AddListener(CloseEndPopup);
        Navigation navigation = new Navigation { mode = Navigation.Mode.None };
        endAcceptButton.navigation = navigation;
        endCloseButton.navigation = navigation;

        Canvas parentCanvas = tutorialEndPopup.GetComponentInParent<Canvas>(true).rootCanvas;
        // 독립 Canvas도 부모 UIBlur와 같은 카메라가 렌더하도록 레이어를 맞춘다.
        // 팝업 프리팹은 UI(5), UIBlurCamera는 UIBlur(8)만 렌더한다.
        foreach (Transform child in tutorialEndPopup.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = parentCanvas.gameObject.layer;

        Canvas popupCanvas = tutorialEndPopup.GetComponent<Canvas>();
        if (popupCanvas == null)
            popupCanvas = tutorialEndPopup.AddComponent<Canvas>();
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingLayerID = parentCanvas.sortingLayerID;
        popupCanvas.sortingOrder = parentCanvas.sortingOrder + 2;
        if (tutorialEndPopup.GetComponent<GraphicRaycaster>() == null)
            tutorialEndPopup.AddComponent<GraphicRaycaster>();
    }

    private void OpenEndPopup()
    {
        if (endPopupOpen || tutorialEndPopup == null || endCloseButton == null)
            return;

        endPopupOpen = true;
        swapHint?.Stop();
        HideHintCircle();
        UpdateScreenNextInput();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        if (UseUnscaledPuzzleTime && puzzleGenerator != null)
            puzzleGenerator.SetTutorialEffectsPaused(true);
        previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        tutorialEndPopup.SetActive(true);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(endCloseButton.gameObject);
    }

    private void AcceptEndPopup()
    {
        Debug.Log("TutorialManager: 튜토리얼 종료 확인 버튼 클릭 (종료 동작 미구현).", this);
    }

    private void CloseEndPopup()
    {
        if (!endPopupOpen)
            return;

        endPopupOpen = false;
        if (puzzleGenerator != null)
            puzzleGenerator.SetTutorialEffectsPaused(false);
        if (tutorialEndPopup != null)
            tutorialEndPopup.SetActive(false);
        if (ownsTimeScale)
            ApplyTutorialTimeScale();
        else
            Time.timeScale = previousTimeScale;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(previousSelection);
        UpdateScreenNextInput();
        TryStartSwapHint();
    }

    private bool ConfigureScreenNextInput()
    {
        Canvas parentCanvas = tutorialEndButton != null
            ? tutorialEndButton.GetComponentInParent<Canvas>(true)
            : null;
        if (parentCanvas == null)
        {
            Debug.LogError("TutorialManager: 종료 버튼을 기존 Canvas 안에 연결해 주세요.", this);
            return false;
        }

        if (nextButton != null)
            nextButton.interactable = false;
        if (underNextButton != null)
            underNextButton.interactable = false;

        Canvas rootCanvas = parentCanvas.rootCanvas;
        var inputObject = new GameObject("TutorialScreenNextInput",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TutorialScreenNextInput));
        inputObject.SetActive(false);
        inputObject.layer = rootCanvas.gameObject.layer;
        inputObject.transform.SetParent(rootCanvas.transform, false);
        inputObject.transform.SetAsLastSibling();

        var rect = (RectTransform)inputObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        screenNextInput = inputObject.GetComponent<TutorialScreenNextInput>();
        screenNextInput.Configure(this, tutorialEndButton.GetComponent<RectTransform>());
        return true;
    }

    private void UpdateScreenNextInput()
    {
        if (screenNextInput == null)
            return;

        bool canAdvance = isActiveAndEnabled && !endPopupOpen && !nextClicked &&
                          currentStep != null && waitingForDialogue;
        screenNextInput.gameObject.SetActive(canAdvance);
    }

    private Vector2Int[] GetSwapHighlightCells(TutorialStep step)
    {
        if (step == steps[0])
            return firstStepHighlightCells;

        return step.dialogueIndex switch
        {
            5 => step5HighlightCells,
            7 => step7HighlightCells,
            12 => step12HighlightCells,
            _ => null
        };
    }

    // 하이라이트에는 현재 단계의 대사 패널과 종료 버튼을 항상 포함합니다.
    private void ShowStepHighlights(TutorialStep step, IEnumerable<Vector2Int> cells)
    {
        if (highlightMask == null)
            return;

        CanvasGroup panel = step.panel == PanelType.Under ? underTutorialPanel : tutorialPanel;
        highlightMask.ShowHighlights(
            new[] { panel != null ? panel.gameObject : null, tutorialEndButton },
            puzzleGenerator, cells);
    }

    private void ShowStep(TutorialStep step)
    {
        bool useUnderPanel = step.panel == PanelType.Under;
        TextMeshProUGUI text = useUnderPanel ? underTutorialText : tutorialText;
        text.text = tutorialTextList.GetDialogue(step.dialogueIndex);

        SetPanel(tutorialPanel, !useUnderPanel, false);
        SetPanel(underTutorialPanel, useUnderPanel, false);
        UpdateScreenNextInput();
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
        if (isActiveAndEnabled && !endPopupOpen && currentStep != null && waitingForDialogue)
        {
            nextClicked = true;
            UpdateScreenNextInput();
        }
    }

    private bool IsExpectedSwap(Vector2Int from, Vector2Int to)
    {
        return currentStep != null && currentStep.waitType == WaitType.Swap &&
               !actionCompleted &&
               ((from == currentStep.from && to == currentStep.to) ||
                (from == currentStep.to && to == currentStep.from));
    }

    private bool IsSwapHintWaiting => isActiveAndEnabled && currentStep != null &&
        currentStep.waitType == WaitType.Swap && waitingForPuzzleAction && !endPopupOpen;

    private void TryStartSwapHint()
    {
        if (!IsSwapHintWaiting || hintPressedTile != null || swapHint == null ||
            puzzleGenerator == null || !puzzleGenerator.IsBoardReady)
            return;

        if (puzzleGenerator.TryGetTutorialPuzzle(currentStep.from, out PuzzleObject first) &&
            puzzleGenerator.TryGetTutorialPuzzle(currentStep.to, out PuzzleObject second) &&
            first is NormalPuzzleObject firstNormal && second is NormalPuzzleObject secondNormal)
        {
            swapHint.Play(firstNormal, secondNormal, swapHintBrightness, swapHintScaleAmount,
                swapHintPulseDuration, swapHintPulseGap, swapHintRepeatDelay, swapHintGlowColor);
            ShowFirstHintCircle();
        }
    }

    private void ShowFirstHintCircle()
    {
        // 이번 테스트는 첫 교환만 적용합니다. 매치 줄 밖의 타일은 step.from = (3,4)입니다.
        if (currentStep != steps[0] || hintCirclePrefab == null || highlightMask == null)
            return;

        RectTransform overlay = highlightMask.DimRectTransform;
        if (overlay == null ||
            !puzzleGenerator.TryGetTutorialCellRect(currentStep.from, out RectTransform frame, out Rect cell))
            return;

        Canvas sourceCanvas = frame.GetComponentInParent<Canvas>().rootCanvas;
        Canvas overlayCanvas = overlay.GetComponentInParent<Canvas>().rootCanvas;
        Camera sourceCamera = sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : sourceCanvas.worldCamera;
        Camera overlayCamera = overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : overlayCanvas.worldCamera;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(
            sourceCamera, frame.TransformPoint(cell.center));
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay, screen, overlayCamera, out Vector2 local))
            return;

        if (hintCircleInstance == null)
        {
            GameObject instance = Instantiate(hintCirclePrefab, overlay, false);
            hintCircleInstance = instance.GetComponent<RectTransform>();
            if (hintCircleInstance == null)
            {
                Destroy(instance);
                Debug.LogError("TutorialManager: Hint Circle에는 RectTransform이 있는 UI 프리팹을 지정해 주세요.", this);
                return;
            }

            // DIM의 자식으로 앞에 표시하고, 기존 Canvas 구조와 퍼즐 입력은 유지합니다.
            foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = overlay.gameObject.layer;
            foreach (Graphic graphic in instance.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        // 프리팹의 크기/색상은 유지하고 저장된 위치 오프셋만 타일 중앙으로 바꿉니다.
        hintCircleInstance.anchorMin = overlay.pivot;
        hintCircleInstance.anchorMax = overlay.pivot;
        hintCircleInstance.pivot = new Vector2(0.5f, 0.5f);
        hintCircleInstance.anchoredPosition3D = new Vector3(local.x, local.y, 0f);
        hintCircleInstance.SetAsLastSibling();
        hintCircleInstance.gameObject.SetActive(true);
    }

    private void HideHintCircle()
    {
        if (hintCircleInstance != null)
            hintCircleInstance.gameObject.SetActive(false);
    }

    private void DestroyHintCircle()
    {
        if (hintCircleInstance == null)
            return;
        hintCircleInstance.gameObject.SetActive(false);
        Destroy(hintCircleInstance.gameObject);
        hintCircleInstance = null;
    }

    private void HandlePuzzlePointerDown(PuzzleObject puzzle)
    {
        if (!IsSwapHintWaiting)
            return;

        hintPressedTile = puzzle;
        swapHint?.Stop();
        HideHintCircle();
    }

    private void HandlePuzzlePointerUp(PuzzleObject puzzle)
    {
        if (hintPressedTile != puzzle)
            return;

        hintPressedTile = null;
        TryStartSwapHint();
    }

    private void HandleSwapStarted(Vector2Int from, Vector2Int to)
    {
        if (waitingForPuzzleAction && !endPopupOpen && IsExpectedSwap(from, to))
            BeginPuzzleAction();
    }

    private void BeginPuzzleAction()
    {
        swapHint?.Stop();
        DestroyHintCircle();
        hintPressedTile = null;
        waitingForPuzzleAction = false;
        puzzleActionRunning = true;
        puzzleGenerator.SetTutorialInputRestriction(true);
        // 매치와 에너지 이동은 별도 퍼즐 시간으로 진행하며 전투는 정지 상태를 유지합니다.
    }

    private void HandleFriendlySpawned(FriendlyUnit unit)
    {
        if (puzzleActionRunning)
            SetTutorialPaused(false);
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

    private void HandleSpecialTileStarted(Vector2Int position, SpecialPuzzleType type)
    {
        if (!waitingForPuzzleAction || endPopupOpen || !IsExpectedSpecialTile(position, type))
            return;

        if (type == SpecialPuzzleType.CircleBomb)
            friendlyGradeOverride = 2;
        BeginPuzzleAction();
    }

    private void HandleSpecialTile(Vector2Int position, SpecialPuzzleType type)
    {
        if (!IsExpectedSpecialTile(position, type))
            return;

        if (type == SpecialPuzzleType.CircleBomb)
            friendlyGradeOverride = 0;

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
        swapHint?.Stop();
        DestroyHintCircle();
        hintPressedTile = null;
        waitingForDialogue = false;
        waitingForPuzzleAction = false;
        puzzleActionRunning = false;

        if (highlightMask != null)
            highlightMask.HideHighlights();

        CloseEndPopup();
        UpdateScreenNextInput();
        if (endButton != null)
            endButton.onClick.RemoveListener(OpenEndPopup);
        if (endAcceptButton != null)
            endAcceptButton.onClick.RemoveListener(AcceptEndPopup);
        if (endCloseButton != null)
            endCloseButton.onClick.RemoveListener(CloseEndPopup);
        StopAllCoroutines();
        currentStep = null;
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextDialogue);
        if (underNextButton != null)
            underNextButton.onClick.RemoveListener(NextDialogue);
        if (puzzleGenerator != null)
        {
            puzzleGenerator.OnSwapStarted -= HandleSwapStarted;
            puzzleGenerator.OnPuzzlePointerDown -= HandlePuzzlePointerDown;
            puzzleGenerator.OnPuzzlePointerUp -= HandlePuzzlePointerUp;
            puzzleGenerator.OnSuccessfulSwapResolved -= HandleSuccessfulSwap;
            puzzleGenerator.OnSpecialTileResolved -= HandleSpecialTile;
            puzzleGenerator.OnSpecialTileStarted -= HandleSpecialTileStarted;
        }
        if (friendlySpawner != null)
            friendlySpawner.OnFriendlySpawned -= HandleFriendlySpawned;
        ReleaseInputRestriction();
        RestoreTutorialTimeScale();
    }

    private void OnDestroy()
    {
        swapHint?.Stop();
        DestroyHintCircle();
        RestoreTutorialTimeScale();
        if (screenNextInput != null)
            Destroy(screenNextInput.gameObject);

        if (activeTutorial == this)
            activeTutorial = null;

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
