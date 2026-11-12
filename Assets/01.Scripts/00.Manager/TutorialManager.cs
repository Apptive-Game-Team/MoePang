using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using _01.Scripts._02.Unit;
using _01.Scripts._04.UI.InGame;
using _01.Scripts._11.HabitatMode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

// StageManager의 Awake 이후, 전투 오브젝트의 Start 이전에 스테이지를 적용한다.
[DefaultExecutionOrder(100)]
public class TutorialManager : MonoBehaviour
{
    public enum WaitType { Swap, NextClick, Delay, Hold, SpecialTile, Event }
    public enum PanelType { Top, Under }
    public enum StepAction { None, CreateObstacle }

    [Serializable]
    public class TutorialStep
    {
        [Tooltip("직접 사용할 번역 키입니다. 비워 두면 기존 대사 목록의 인덱스를 사용합니다.")]
        public string dialogueKey;
        [Tooltip("Dialogue Key가 비어 있을 때만 사용하는 기존 대사 목록 인덱스입니다.")]
        [Min(0)] public int dialogueIndex;
        public WaitType waitType;
        [Tooltip("Event 단계에서 누를 버튼입니다. 클릭 시 현재 동작을 완료합니다.")]
        public Button actionButton;
        [Header("팝업 설명 중 임시 배치")]
        [Tooltip("이 Image만 투명하게 하며, 연속된 팝업 설명이 끝나면 색상과 위치를 복원하고 팝업을 닫습니다.")]
        public Image popupBackground;
        public Vector2 popupOffset;
        public Vector2 dialoguePanelOffset;
        public PanelType panel;
        public Vector2Int from;
        public Vector2Int to;
        public SpecialPuzzleType specialType = SpecialPuzzleType.RowBomb;
        public StepAction enterAction;
        public Vector2Int obstaclePosition;
        public ObstaclePuzzleType obstacleType;
        [Min(0f)] public float delay = 1f;

        [Header("표시와 하이라이트")]
        [Tooltip("-1이면 공통 대사 시간을 사용합니다.")]
        public float dialogueDuration = -1f;
        public bool dimBackground;
        public GameObject[] highlightObjects = Array.Empty<GameObject>();
        public RectTransform[] highlightRects = Array.Empty<RectTransform>();
        public Vector2Int[] highlightCells = Array.Empty<Vector2Int>();
        public bool highlightSpawnStack;
        public bool showCameraButton;
        public bool focusSpawnedUnit;
        [Min(1)] public int focusUnitGrade = 2;
        public bool playHintAnimation = true;
        public bool showHintCircle;
        public bool hidePanelBeforeSpawn = true;

        [Header("동작 중 소환 설정")]
        [Tooltip("-1이면 등급 설정을 유지, 0이면 일반 소환 규칙을 사용합니다.")]
        public int actionFriendlyGrade = -1;
        public int afterActionFriendlyGrade = -1;
        public bool recordSpawnedUnits;

        [Header("추가 이벤트 (설명 다시 보기에서도 실행됩니다)")]
        public UnityEvent onEnter = new();
        public UnityEvent onActionStarted = new();
        public UnityEvent onExit = new();
    }

    [Header("실행 방식")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool requireTutorialMode = true;
    [Tooltip("일반 UI 안내에서는 끄세요. 퍼즐, 전투 스탯, 소환 제어를 사용하지 않습니다.")]
    [SerializeField] private bool useBattleRules = true;
    [SerializeField] private bool pauseGameplay = true;
    [SerializeField] private string localizationTable = "LocalizationDataTable";
    [SerializeField] private UnityEvent onCompleted = new();
    [SerializeField] private UnityEvent onStopped = new();
    public bool IsRunning { get; private set; }
    public bool IsInstructionComplete { get; private set; }
    public int CurrentStepIndex => currentStep != null ? currentStepIndex : -1;

    [Header("Tutorial Stage")]
    [Tooltip("내부 스테이지 번호입니다. 0 = 화면의 1스테이지(최저 난이도).")]
    [Min(0)] [SerializeField] private int tutorialStage = 0;
    [SerializeField] private bool applyTutorialStage = true;
    [Min(0)] [SerializeField] private int initialFriendlyGrade = 1;
    [Min(1f)] [SerializeField] private float tutorialCastleHp = 500f;
    [SerializeField] private bool mirrorEnemySpawns = true;
    [SerializeField] private int[] tutorialEnemyIndices = { 0, 2 };

    [Header("Tutorial Enemy Stats")]
    [Min(1f)] [SerializeField] private float tutorialEnemyMaxHp = 16f;
    [Min(0f)] [SerializeField] private float tutorialEnemyAttackDamage = 0f;

    [Header("대사 진행")]
    [Tooltip("대사 표시 후 다음 이벤트까지 기다리는 시간입니다. 화면을 누르면 즉시 진행합니다.")]
    [Min(0f)] [SerializeField] private float dialogueDuration = 5f;

    [Tooltip("퍼즐 처리와 모든 소환이 끝난 뒤 다음 대사까지 기다리는 시간입니다.")]
    [Min(0f)] [SerializeField] private float postActionDelay = 2f;

    private static TutorialManager activeTutorial;
    private static string pendingRestartScene;
    private static string pendingRestartOwner;
    private bool sessionPrepared;
    private int runVersion;
    private bool habitatApplied;
    private readonly Dictionary<CanvasGroup, UiState> savedUiStates = new();

    private struct UiState
    {
        public bool active;
        public float alpha;
        public bool interactable;
        public bool blocksRaycasts;
    }

    private static bool HasActiveBattle => activeTutorial != null &&
        activeTutorial.isActiveAndEnabled && activeTutorial.sessionPrepared &&
        activeTutorial.useBattleRules && GameManager.Instance != null && GameManager.Instance.Istutorial;
    // 0: 기존 소환 규칙, 1: 햇빛 이전, 2: 햇빛 사용으로 발생하는 소환.
    private int friendlyGradeOverride = 1;

    public static int FriendlyGradeOverride =>
        HasActiveBattle
            ? activeTutorial.friendlyGradeOverride : 0;

    public static bool UseUnscaledPuzzleTime => activeTutorial != null &&
        activeTutorial.isActiveAndEnabled && activeTutorial.ownsTimeScale &&
        activeTutorial.usesUnscaledPuzzleTime;

    public static bool IsTutorialPopupOpen => UseUnscaledPuzzleTime && activeTutorial.endPopupOpen;

    public static bool TryGetFriendlyUnitOverride(
        IReadOnlyList<FriendlyUnitData> candidates, out FriendlyUnitData unit, int grade)
    {
        unit = null;
        if (!HasActiveBattle || grade == 0)
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

    public static bool TryGetCastleHp(out float maxHp)
    {
        maxHp = HasActiveBattle ? Mathf.Max(1f, activeTutorial.tutorialCastleHp) : 0f;
        return HasActiveBattle;
    }

    public static bool TryGetEnemyStats(out float maxHp, out float attackDamage)
    {
        maxHp = 0f;
        attackDamage = 0f;
        if (!HasActiveBattle)
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
    [SerializeField] private Button tutorialTextReplayButton;
    // 기존 씬의 참조는 보존하되, 대사 진행 입력은 화면 전체 영역에서 받습니다.
    [SerializeField, HideInInspector] private Button nextButton;
    [SerializeField, HideInInspector] private Button underNextButton;

    [Header("공통 하이라이트 대상")]
    [Tooltip("서식지 아이콘과 에너지 원을 포함하는 SpawnStack 부모 영역입니다.")]
    [SerializeField] private RectTransform spawnStackHighlightRoot;

    [Header("유닛 집중 카메라")]
    [SerializeField] private BattleUIController battleCameraController;

    [Header("하이라이트")]
    [SerializeField] private TutorialHighlightMask highlightMask;

    [Header("교환 원형 힌트")]
    [Tooltip("Show Hint Circle을 켠 단계에서 사용할 UI 프리팹입니다.")]
    [SerializeField] private GameObject hintCirclePrefab;

    [Header("교환 타일 힌트 애니메이션")]
    [Range(0f, 1f)] [SerializeField] private float swapHintBrightness = 0.6f;
    [Range(0f, 0.3f)] [SerializeField] private float swapHintScaleAmount = 0.08f;
    [Min(0.05f)] [SerializeField] private float swapHintPulseDuration = 0.25f;
    [Min(0f)] [SerializeField] private float swapHintPulseGap = 0.12f;
    [Min(0f)] [SerializeField] private float swapHintRepeatDelay = 1f;
    [Tooltip("타일이 밝아질 때 함께 표시하는 외곽 글로우 색입니다. HDR 색상과 알파로 강도를 조절합니다.")]
    [ColorUsage(true, true)] [SerializeField] private Color swapHintGlowColor = new Color(5f, 4.5f, 2.5f, 0.65f);

    [SerializeField] private TutorialStep[] steps = Array.Empty<TutorialStep>();

    private TutorialStep currentStep;
    private bool actionCompleted;
    private bool nextClicked;
    private bool waitingForDialogue;
    private bool waitingForPuzzleAction;
    private bool waitingForEventAction;
    private Button currentActionButton;
    private Image explainingPopup;
    private RectTransform explainingPanel;
    private Color originalPopupColor;
    private Vector2 originalPopupPosition;
    private Vector2 originalPanelPosition;
    private bool ownsTimeScale;
    private bool usesUnscaledPuzzleTime;
    private bool puzzleActionRunning;
    private UnitSpawner friendlySpawner;
    private readonly List<FriendlyUnit> recordedSpawnedUnits = new();
    private readonly Dictionary<SpriteRenderer, int> highlightedUnitSortingOrders = new();
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
    [SerializeField] private Button endAcceptButton;
    [SerializeField] private Button endCloseButton;
    private GameObject previousSelection;
    private bool endPopupOpen;
    private float previousTimeScale;
    private TutorialScreenNextInput screenNextInput;
    private TutorialSwapHintAnimation swapHint;
    private PuzzleObject hintPressedTile;
    private RectTransform hintCircleInstance;
    private int currentStepIndex;
    private int replayStartIndex = -1;
    private bool replayRequested;
    private bool isReplaying;
    private bool presentationFailed;

    private void Awake()
    {
        if (highlightMask == null)
            highlightMask = GetComponent<TutorialHighlightMask>();
        swapHint = new TutorialSwapHintAnimation();

        // 전투 오브젝트의 Start보다 먼저 스테이지와 스탯 재정의를 준비합니다.
        if ((autoStart || HasPendingRestart) && CanStartInCurrentMode)
            PrepareSession();
    }

    private bool CanStartInCurrentMode => !requireTutorialMode ||
        (GameManager.Instance != null && GameManager.Instance.Istutorial);

    private bool HasPendingRestart => pendingRestartScene == gameObject.scene.path &&
        pendingRestartOwner == name;

    private void PrepareSession()
    {
        if (sessionPrepared || (activeTutorial != null && activeTutorial != this))
            return;

        activeTutorial = this;
        sessionPrepared = true;
        friendlyGradeOverride = initialFriendlyGrade;
        if (!useBattleRules)
            return;

        stageManager = StageManager.Instance;
        if (applyTutorialStage && stageManager != null)
        {
            previousStage = stageManager.CurrentStage;
            stageManager.SetStage(Mathf.Max(0, tutorialStage));
            stageApplied = true;
        }
        habitatModeManager = HabitatModeManager.Instance;
        if (habitatModeManager != null)
        {
            previousHabitatBattle = habitatModeManager.IsHabitatBattle;
            habitatModeManager.IsHabitatBattle = false;
            habitatApplied = true;
        }

        if (puzzleGenerator == null)
            puzzleGenerator = FindFirstObjectByType<PuzzleGenerator>();

        // Start 순서와 관계없이 보드 생성 중의 입력도 차단한다.
        if (puzzleGenerator != null)
        {
            puzzleGenerator.SetTutorialInputRestriction(true);
            ownsInputRestriction = true;
        }
    }

    private void Start()
    {
        if (IsRunning)
            return;

        highlightMask?.HideHighlights();
        SetPanel(tutorialPanel, false, false);
        SetPanel(underTutorialPanel, false, false);
        if (tutorialEndPopup != null)
            tutorialEndPopup.SetActive(false);
        if (tutorialEndButton != null)
            tutorialEndButton.SetActive(false);
        SetReplayButtonVisible(false);

        bool restart = HasPendingRestart;
        if (restart)
        {
            pendingRestartScene = null;
            pendingRestartOwner = null;
        }
        if ((autoStart || restart) && CanStartInCurrentMode)
            StartTutorial();
    }

    /// <summary>현재 씬에서 설정된 안내를 시작합니다. 전투 보드를 초기화하지는 않습니다.</summary>
    public void StartTutorial()
    {
        if (IsRunning || !isActiveAndEnabled || !CanStartInCurrentMode)
            return;
        if (activeTutorial != null && activeTutorial != this)
        {
            Debug.LogError("TutorialManager: 다른 튜토리얼을 먼저 중지해 주세요.", this);
            return;
        }

        if (useBattleRules && puzzleGenerator == null)
            puzzleGenerator = FindFirstObjectByType<PuzzleGenerator>();
        if (!ValidateSetup() || !ConfigureScreenNextInput())
        {
            StopTutorial();
            return;
        }
        PrepareSession();
        IsRunning = true;
        IsInstructionComplete = false;
        presentationFailed = false;
        actionCompleted = false;
        replayRequested = false;
        isReplaying = false;
        currentStep = null;
        currentStepIndex = 0;
        recordedSpawnedUnits.Clear();
        SaveUiState(timeInfo);
        SaveUiState(lockButton);
        SaveUiState(pauseButton);
        ConfigureEndPopup();
        if (tutorialEndButton != null)
            tutorialEndButton.SetActive(true);
        if (tutorialTextReplayButton != null)
        {
            tutorialTextReplayButton.navigation = new Navigation { mode = Navigation.Mode.None };
            tutorialTextReplayButton.onClick.AddListener(ReplayExplanation);
        }
        if (useBattleRules)
        {
            puzzleGenerator.OnSwapStarted += HandleSwapStarted;
            puzzleGenerator.OnPuzzlePointerDown += HandlePuzzlePointerDown;
            puzzleGenerator.OnPuzzlePointerUp += HandlePuzzlePointerUp;
            puzzleGenerator.OnSuccessfulSwapResolved += HandleSuccessfulSwap;
            puzzleGenerator.OnSpecialTileResolved += HandleSpecialTile;
            puzzleGenerator.OnSpecialTileStarted += HandleSpecialTileStarted;
            friendlySpawner = puzzleGenerator.FriendlySpawner;
        }
        if (friendlySpawner != null)
        {
            friendlySpawner.OnFriendlySpawning += HandleFriendlySpawning;
            friendlySpawner.OnFriendlySpawned += HandleFriendlySpawned;
        }
        SetUi(timeInfo, 0f);
        SetUi(lockButton, 0f);
        SetUi(pauseButton, 0f);
        SetPanel(tutorialPanel, false, false);
        SetPanel(underTutorialPanel, false, false);

        originalTimeScale = Time.timeScale;
        gameplayTimeScale = originalTimeScale > 0f ? originalTimeScale : 1f;
        ownsTimeScale = pauseGameplay;
        usesUnscaledPuzzleTime = useBattleRules;
        StartCoroutine(RunTutorial(++runVersion));
    }

    /// <summary>이벤트 구독, 입력 제한과 임시 설정을 해제합니다.</summary>
    public void StopTutorial()
    {
        EndSession(true);
    }

    private void EndSession(bool notifyStopped)
    {
        bool wasRunning = IsRunning;
        IsRunning = false;
        runVersion++;
        StopAllCoroutines();
        CleanUpSession();
        if (wasRunning && notifyStopped)
            onStopped?.Invoke();
    }

    /// <summary>전투는 보드·유닛까지 초기화하도록 씬을 재로드하고, UI 안내는 현재 씬에서 재시작합니다.</summary>
    public void RestartTutorial()
    {
        if (!isActiveAndEnabled || !CanStartInCurrentMode)
            return;
        if (useBattleRules)
        {
            string scenePath = gameObject.scene.path;
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("TutorialManager: 전투 재시작에는 저장된 씬이 필요합니다.", this);
                return;
            }
            pendingRestartScene = scenePath;
            pendingRestartOwner = name;
            StopTutorial();
            SceneManager.LoadScene(scenePath);
        }
        else
        {
            StopTutorial();
            StartTutorial();
        }
    }

    /// <summary>Event 대기 단계에서 외부 버튼이나 시스템이 호출합니다.</summary>
    public void CompleteCurrentAction()
    {
        if (IsRunning && currentStep != null && currentStep.waitType == WaitType.Event &&
            waitingForEventAction && !isReplaying && !replayRequested && !endPopupOpen)
        {
            actionCompleted = true;
            UpdateScreenNextInput();
        }
    }

    private IEnumerator RunTutorial(int version)
    {
        // 초기 보드의 낙하가 끝난 뒤 첫 대사를 표시합니다.
        SetTutorialPaused(false);
        yield return new WaitUntil(CanContinueTutorial);

        for (currentStepIndex = 0; currentStepIndex < steps.Length; currentStepIndex++)
        {
            TutorialStep step = steps[currentStepIndex];
            replayStartIndex = GetReplayStartIndex(currentStepIndex);
            actionCompleted = false;
            puzzleActionRunning = false;
            yield return PresentStep(step);
            if (presentationFailed || !IsRunning || version != runVersion)
                yield break;

            switch (step.waitType)
            {
                case WaitType.Swap:
                case WaitType.SpecialTile:
                    yield return WaitForPuzzleResolution();
                    if (presentationFailed)
                        yield break;
                    break;
                case WaitType.NextClick:
                    break;
                case WaitType.Delay:
                    yield return WaitTutorialDelay(step.delay);
                    break;
                case WaitType.Event:
                    yield return WaitForEventAction();
                    break;
                case WaitType.Hold:
                    // 마지막 안내 후에는 전투와 자유 퍼즐 조작을 계속할 수 있습니다.
                    SetPanel(tutorialPanel, false, false);
                    SetPanel(underTutorialPanel, false, false);
                    ClearStepPresentation();
                    usesUnscaledPuzzleTime = false;
                    SetTutorialPaused(false);
                    ReleaseInputRestriction();
                    currentStep = null;
                    IsInstructionComplete = true;
                    step.onExit?.Invoke();
                    if (IsRunning && version == runVersion)
                        onCompleted?.Invoke();
                    yield break;
            }
            if (presentationFailed || !IsRunning || version != runVersion)
                yield break;
            step.onExit?.Invoke();
            if (!IsRunning || version != runVersion)
                yield break;
        }

        EndSession(false);
        IsInstructionComplete = true;
        onCompleted?.Invoke();
    }

    private static bool IsPuzzleActionStep(TutorialStep step) =>
        step.waitType == WaitType.Swap || step.waitType == WaitType.SpecialTile;

    private int GetReplayStartIndex(int actionIndex)
    {
        if (!IsPuzzleActionStep(steps[actionIndex]) && steps[actionIndex].waitType != WaitType.Event)
            return -1;

        int start = actionIndex;
        while (start > 0 && (steps[start - 1].waitType == WaitType.NextClick ||
                             steps[start - 1].waitType == WaitType.Delay))
            start--;
        return start < actionIndex ? start : -1;
    }

    private void ClearStepPresentation()
    {
        ReleaseActionButton();
        RestoreHighlightedUnitSortingOrders();
        battleCameraController?.EndTutorialFocus();
        highlightMask?.HideHighlights();
        swapHint?.Stop();
        DestroyHintCircle();
        hintPressedTile = null;
        SetReplayButtonVisible(false);
    }

    private IEnumerator PresentStep(TutorialStep step)
    {
        int version = runVersion;
        ClearStepPresentation();
        SetTutorialPaused(true);
        currentStep = step;
        ApplyPopupPresentation(step);
        waitingForPuzzleAction = false;
        nextClicked = false;
        if (useBattleRules)
            puzzleGenerator.SetTutorialInputRestriction(true);
        waitingForDialogue = !IsPuzzleActionStep(step) && step.waitType != WaitType.Event &&
            step.enterAction != StepAction.CreateObstacle;
        ShowStep(step);
        step.onEnter?.Invoke();
        if (!IsRunning || version != runVersion)
            yield break;

        if (step.enterAction == StepAction.CreateObstacle)
        {
            yield return new WaitUntil(CanContinueTutorial);
            if (!puzzleGenerator.TryCreateTutorialObstacle(step.obstaclePosition, step.obstacleType))
            {
                FailTutorialPresentation();
                yield break;
            }
            yield return new WaitUntil(CanContinueTutorial);
            SetTutorialPaused(true);
            // 생성이 끝난 시점부터 대사 대기를 시작하고 화면 클릭도 허용합니다.
            waitingForDialogue = !IsPuzzleActionStep(step) && step.waitType != WaitType.Event;
            UpdateScreenNextInput();
        }
        if (step.waitType == WaitType.Event)
        {
            waitingForEventAction = true;
            currentActionButton = step.actionButton;
            if (currentActionButton != null)
                currentActionButton.onClick.AddListener(CompleteCurrentAction);
            SetReplayButtonVisible(replayStartIndex >= 0 && !isReplaying);
            UpdateScreenNextInput();
            ShowStepHighlights(step, step.highlightCells);
            yield break;
        }
        if (IsPuzzleActionStep(step))
        {
            // 별도 생성 연출이 없다면 대사 표시와 같은 프레임에 조작과 힌트를 활성화합니다.
            BeginPuzzleActionWait();
            yield break;
        }
        yield return WaitForDialogue();
    }

    private void ReleaseActionButton()
    {
        waitingForEventAction = false;
        if (currentActionButton != null)
            currentActionButton.onClick.RemoveListener(CompleteCurrentAction);
        currentActionButton = null;
    }

    private void ApplyPopupPresentation(TutorialStep step)
    {
        CanvasGroup panel = step.panel == PanelType.Under ? underTutorialPanel : tutorialPanel;
        RectTransform panelRect = panel != null ? panel.GetComponent<RectTransform>() : null;
        if (explainingPopup == step.popupBackground && explainingPanel == panelRect)
            return;

        RestorePopupPresentation();
        if (step.popupBackground == null)
            return;

        explainingPopup = step.popupBackground;
        explainingPanel = panelRect;
        originalPopupColor = explainingPopup.color;
        originalPopupPosition = explainingPopup.rectTransform.anchoredPosition;
        if (explainingPanel != null)
        {
            originalPanelPosition = explainingPanel.anchoredPosition;
            explainingPanel.anchoredPosition = originalPanelPosition + step.dialoguePanelOffset;
        }

        Color transparent = originalPopupColor;
        transparent.a = 0f;
        explainingPopup.color = transparent;
        explainingPopup.rectTransform.anchoredPosition = originalPopupPosition + step.popupOffset;
        explainingPopup.gameObject.SetActive(true);
    }

    private void RestorePopupPresentation()
    {
        if (explainingPopup != null)
        {
            explainingPopup.color = originalPopupColor;
            explainingPopup.rectTransform.anchoredPosition = originalPopupPosition;
            explainingPopup.gameObject.SetActive(false);
        }
        if (explainingPanel != null)
            explainingPanel.anchoredPosition = originalPanelPosition;
        explainingPopup = null;
        explainingPanel = null;
    }

    private IEnumerator WaitForEventAction()
    {
        while (endPopupOpen || !actionCompleted)
        {
            if (replayRequested && !endPopupOpen)
            {
                yield return ReplayExplanationRange();
                if (presentationFailed)
                    yield break;
            }
            yield return null;
        }
        ReleaseActionButton();
        SetReplayButtonVisible(false);
    }

    private void BeginPuzzleActionWait()
    {
        waitingForPuzzleAction = true;
        SetReplayButtonVisible(replayStartIndex >= 0 && !isReplaying);
        if (currentStep.waitType == WaitType.Swap)
        {
            Vector2Int[] cells = currentStep.highlightCells != null && currentStep.highlightCells.Length > 0
                ? currentStep.highlightCells : new[] { currentStep.from, currentStep.to };
            ShowStepHighlights(currentStep, cells);
            puzzleGenerator.SetTutorialInputRestriction(true,
                (from, to) => !endPopupOpen && waitingForPuzzleAction && IsExpectedSwap(from, to));
            TryStartSwapHint();
        }
        else
        {
            ShowStepHighlights(currentStep,
                currentStep.highlightCells != null && currentStep.highlightCells.Length > 0
                    ? currentStep.highlightCells : new[] { currentStep.from });
            puzzleGenerator.SetTutorialInputRestriction(true,
                specialAllowed: (position, type) => !endPopupOpen && waitingForPuzzleAction &&
                                                   IsExpectedSpecialTile(position, type));
            TryStartSwapHint();
        }
    }

    private void SetReplayButtonVisible(bool visible)
    {
        if (tutorialTextReplayButton == null)
            return;
        bool changed = tutorialTextReplayButton.gameObject.activeSelf != visible;
        tutorialTextReplayButton.gameObject.SetActive(visible);
        tutorialTextReplayButton.interactable = visible && !endPopupOpen;
        if (changed)
            highlightMask?.RequestRefresh();
        UpdateScreenNextInput();
    }

    public void ReplayExplanation()
    {
        if (!isActiveAndEnabled || (!waitingForPuzzleAction && !waitingForEventAction) ||
            endPopupOpen || isReplaying || replayRequested || replayStartIndex < 0 || actionCompleted ||
            (useBattleRules && (puzzleGenerator == null || !puzzleGenerator.IsBoardReady)))
            return;

        // 클릭한 프레임부터 조작을 막아 재생 요청과 교환이 동시에 시작되지 않게 합니다.
        replayRequested = true;
        waitingForPuzzleAction = false;
        ReleaseActionButton();
        if (useBattleRules)
            puzzleGenerator.SetTutorialInputRestriction(true);
        swapHint?.Stop();
        DestroyHintCircle();
        SetReplayButtonVisible(false);
    }

    private IEnumerator ReplayExplanationRange()
    {
        int version = runVersion;
        replayRequested = false;
        isReplaying = true;
        ClearStepPresentation();
        yield return new WaitUntil(CanContinueTutorial);

        // 설명 중 만든 장애물만 되돌립니다. 매치, 에너지, 소환 결과는 건드리지 않습니다.
        for (int index = currentStepIndex; index >= replayStartIndex; index--)
        {
            TutorialStep step = steps[index];
            if (step.enterAction == StepAction.CreateObstacle &&
                !puzzleGenerator.TryRestoreTutorialObstacle(step.obstaclePosition, step.obstacleType))
            {
                FailTutorialPresentation();
                yield break;
            }
        }

        for (int index = replayStartIndex; index <= currentStepIndex; index++)
        {
            TutorialStep step = steps[index];
            yield return PresentStep(step);
            if (presentationFailed || !IsRunning || version != runVersion)
                yield break;
            if (step.waitType == WaitType.Delay)
                yield return WaitTutorialDelay(step.delay);
            if (index < currentStepIndex)
                step.onExit?.Invoke();
            if (!IsRunning || version != runVersion)
                yield break;
        }

        isReplaying = false;
        SetReplayButtonVisible((waitingForPuzzleAction || waitingForEventAction) && replayStartIndex >= 0);
        if (waitingForEventAction)
            ShowStepHighlights(currentStep, currentStep.highlightCells);
    }

    private void FailTutorialPresentation()
    {
        presentationFailed = true;
        StopTutorial();
    }

    private bool CanContinueTutorial() => !endPopupOpen &&
        (!useBattleRules || puzzleGenerator.IsBoardReady);

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
        while (endPopupOpen || !actionCompleted || !puzzleGenerator.IsBoardReady)
        {
            if (replayRequested && !endPopupOpen)
            {
                yield return ReplayExplanationRange();
                if (presentationFailed)
                    yield break;
            }
            yield return null;
        }
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
        float duration = currentStep.dialogueDuration >= 0f ? currentStep.dialogueDuration : dialogueDuration;
        while (endPopupOpen || (!nextClicked && elapsed < duration))
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
        if (tutorialEndButton == null)
            return;

        // 종료 버튼을 감싸는 부모 오브젝트를 연결해도 실제 자식 버튼에 입력을 연결합니다.
        endButton = tutorialEndButton.GetComponentInChildren<Button>(true);
        if (endButton != null)
            endButton.onClick.AddListener(OpenEndPopup);
        if (tutorialEndPopup == null)
            return;
        if (endAcceptButton == null || endCloseButton == null)
        {
            Debug.LogError("TutorialManager: 종료 팝업의 확인/닫기 버튼을 연결해 주세요.", this);
            return;
        }

        endAcceptButton.onClick.AddListener(AcceptEndPopup);
        endCloseButton.onClick.AddListener(CloseEndPopup);
        Navigation navigation = new Navigation { mode = Navigation.Mode.None };
        endAcceptButton.navigation = navigation;
        endCloseButton.navigation = navigation;

        Canvas parentCanvas = tutorialEndPopup.GetComponentInParent<Canvas>(true)?.rootCanvas;
        if (parentCanvas == null)
            return;
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
        if (!IsRunning || endPopupOpen)
            return;
        if (tutorialEndPopup == null)
        {
            StopTutorial();
            return;
        }
        if (endCloseButton == null)
            return;

        endPopupOpen = true;
        if (tutorialTextReplayButton != null)
            tutorialTextReplayButton.interactable = false;
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
        StopTutorial();
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
        if (tutorialTextReplayButton != null)
            tutorialTextReplayButton.interactable = (waitingForPuzzleAction || waitingForEventAction) && replayStartIndex >= 0 &&
                                                    !isReplaying && !replayRequested;
    }

    private bool ConfigureScreenNextInput()
    {
        Canvas parentCanvas = tutorialEndButton != null
            ? tutorialEndButton.GetComponentInParent<Canvas>(true)
            : tutorialPanel != null ? tutorialPanel.GetComponentInParent<Canvas>(true) : null;
        if (parentCanvas == null)
        {
            Debug.LogError("TutorialManager: 대사 패널을 Canvas 안에 연결해 주세요.", this);
            return false;
        }

        if (nextButton != null)
            nextButton.interactable = false;
        if (underNextButton != null)
            underNextButton.interactable = false;

        Canvas rootCanvas = parentCanvas.rootCanvas;
        if (screenNextInput != null)
        {
            screenNextInput.Configure(this, tutorialEndButton != null
                ? GetEndButtonRect() : null);
            return true;
        }
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
        screenNextInput.Configure(this, tutorialEndButton != null
            ? GetEndButtonRect() : null);
        return true;
    }

    private RectTransform GetEndButtonRect() => endButton != null
        ? endButton.GetComponent<RectTransform>()
        : tutorialEndButton.GetComponentInChildren<Button>(true)?.GetComponent<RectTransform>();

    private void UpdateScreenNextInput()
    {
        if (screenNextInput == null)
            return;

        bool canAdvance = IsRunning && isActiveAndEnabled && !endPopupOpen && !nextClicked &&
                          currentStep != null && waitingForDialogue;
        bool restrictButtons = IsRunning && isActiveAndEnabled && !endPopupOpen &&
                               currentStep != null && currentStep.waitType == WaitType.Event &&
                               currentStep.actionButton != null;
        RectTransform actionRect = waitingForEventAction && !actionCompleted && !isReplaying && !replayRequested &&
                                   currentActionButton != null
            ? currentActionButton.GetComponent<RectTransform>() : null;
        RectTransform replayRect = tutorialTextReplayButton != null && tutorialTextReplayButton.interactable &&
                                   tutorialTextReplayButton.gameObject.activeInHierarchy
            ? tutorialTextReplayButton.GetComponent<RectTransform>() : null;
        screenNextInput.SetAllowedButtons(actionRect, replayRect);
        screenNextInput.gameObject.SetActive(canAdvance || restrictButtons);
    }

    // 하이라이트에는 현재 단계의 대사 패널과 종료 버튼을 항상 포함합니다.
    private void ShowStepHighlights(TutorialStep step, IEnumerable<Vector2Int> cells)
    {
        if (highlightMask == null)
            return;

        CanvasGroup panel = step.panel == PanelType.Under ? underTutorialPanel : tutorialPanel;
        var targets = new List<GameObject>
        {
            panel != null ? panel.gameObject : null, tutorialEndButton,
            tutorialTextReplayButton != null && tutorialTextReplayButton.gameObject.activeInHierarchy
                ? tutorialTextReplayButton.gameObject : null
        };
        if (step.highlightObjects != null)
            targets.AddRange(step.highlightObjects);
        if (step.showCameraButton && lockButton != null)
            targets.Add(lockButton.gameObject);
        if (step.focusSpawnedUnit)
        {
            FriendlyUnit unit = GetFrontRecordedUnit(step.focusUnitGrade);
            if (unit != null)
                targets.Add(unit.gameObject);
        }
        var rects = new List<RectTransform>();
        if (step.highlightRects != null)
            rects.AddRange(step.highlightRects);
        if (step.highlightSpawnStack && spawnStackHighlightRoot != null)
            rects.Add(spawnStackHighlightRoot);
        highlightMask.ShowHighlights(targets, useBattleRules ? puzzleGenerator : null, cells, rects);
    }

    private void ShowStep(TutorialStep step)
    {
        bool useUnderPanel = step.panel == PanelType.Under;
        TextMeshProUGUI text = useUnderPanel ? underTutorialText : tutorialText;
        text.text = !string.IsNullOrWhiteSpace(step.dialogueKey)
            ? LocalizationSettings.StringDatabase.GetLocalizedString(localizationTable, step.dialogueKey)
            : tutorialTextList.GetDialogue(step.dialogueIndex);

        SetPanel(tutorialPanel, !useUnderPanel, false);
        SetPanel(underTutorialPanel, useUnderPanel, false);
        // 시작 시 숨겨 둔 카메라 버튼은 조작 설명 단계에서만 표시합니다.
        bool showCameraButton = step.showCameraButton;
        SetUi(lockButton, showCameraButton ? 1f : 0f, showCameraButton);
        // 버튼의 비활성 색상은 해제하고, 설명 진행 입력은 기존 화면 전체 클릭으로 받습니다.
        if (showCameraButton && lockButton != null)
            lockButton.blocksRaycasts = false;
        UpdateScreenNextInput();

        if (step.focusSpawnedUnit)
        {
            FriendlyUnit unit = GetFrontRecordedUnit(step.focusUnitGrade);
            if (unit != null)
            {
                BringHighlightedUnitToFront(unit);
                battleCameraController?.BeginTutorialFocus(unit.transform);
            }
        }
        if (step.dimBackground || step.highlightSpawnStack || step.showCameraButton ||
            step.focusSpawnedUnit || (step.highlightObjects != null && step.highlightObjects.Length > 0) ||
            (step.highlightRects != null && step.highlightRects.Length > 0) ||
            (step.highlightCells != null && step.highlightCells.Length > 0))
            ShowStepHighlights(step, step.highlightCells);
    }

    private FriendlyUnit GetFrontRecordedUnit(int grade)
    {
        FriendlyUnit frontUnit = null;
        foreach (FriendlyUnit unit in recordedSpawnedUnits)
        {
            if (unit == null || !unit.isActiveAndEnabled || unit.CurrentHp <= 0f ||
                unit.Data == null || unit.Data.UnitGrade != grade)
                continue;

            // 아군은 +X 방향으로 전진하므로 X가 가장 큰 유닛이 선두입니다.
            if (frontUnit == null || unit.transform.position.x > frontUnit.transform.position.x)
                frontUnit = unit;
        }

        return frontUnit;
    }

    private void BringHighlightedUnitToFront(FriendlyUnit unit)
    {
        RestoreHighlightedUnitSortingOrders();
        SpriteRenderer body = unit.GetComponent<SpriteRenderer>();
        if (body == null)
            return;

        // 본체와 자식 스프라이트의 앞뒤 관계를 유지하며 설명 중에만 다른 유닛보다 앞에 그립니다.
        int offset = 32000 - body.sortingOrder;
        foreach (SpriteRenderer renderer in unit.GetComponentsInChildren<SpriteRenderer>(true))
        {
            highlightedUnitSortingOrders.Add(renderer, renderer.sortingOrder);
            renderer.sortingOrder = Mathf.Clamp(renderer.sortingOrder + offset, -32768, 32767);
        }
    }

    private void RestoreHighlightedUnitSortingOrders()
    {
        foreach (var entry in highlightedUnitSortingOrders)
            if (entry.Key != null)
                entry.Key.sortingOrder = entry.Value;
        highlightedUnitSortingOrders.Clear();
    }

    private void LateUpdate()
    {
        // 대사 중 카메라가 이동해도 선택한 유닛 위치에 마스크를 맞춥니다.
        if (waitingForDialogue && currentStep != null && currentStep.focusSpawnedUnit)
            highlightMask?.RequestRefresh();
    }

    private void SetPanel(CanvasGroup panel, bool visible, bool interactable)
    {
        if (panel == null)
            return;

        // 패널의 자식인 종료 버튼도 부모 CanvasGroup의 입력 제한을 받습니다.
        bool containsEndButton = tutorialEndButton != null &&
                                 tutorialEndButton.transform.IsChildOf(panel.transform);
        SetUi(panel, visible ? 1f : 0f, visible && (interactable || containsEndButton));
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

    private bool IsSwapHintWaiting => IsRunning && isActiveAndEnabled && currentStep != null &&
        IsPuzzleActionStep(currentStep) && waitingForPuzzleAction && !endPopupOpen;

    private void TryStartSwapHint()
    {
        if (!IsSwapHintWaiting || hintPressedTile != null || swapHint == null ||
            puzzleGenerator == null || !puzzleGenerator.IsBoardReady)
            return;

        if (currentStep.waitType == WaitType.SpecialTile)
        {
            if (puzzleGenerator.TryGetTutorialPuzzle(currentStep.from, out PuzzleObject puzzle) &&
                puzzle is SpecialPuzzleObject special && special.specialPuzzleType == currentStep.specialType)
            {
                if (currentStep.playHintAnimation)
                    swapHint.Play(special, null, swapHintBrightness, swapHintScaleAmount,
                        swapHintPulseDuration, swapHintPulseGap, swapHintRepeatDelay, swapHintGlowColor);
                ShowSwapHintCircle();
            }
            return;
        }

        if (puzzleGenerator.TryGetTutorialPuzzle(currentStep.from, out PuzzleObject first) &&
            puzzleGenerator.TryGetTutorialPuzzle(currentStep.to, out PuzzleObject second) &&
            first is NormalPuzzleObject firstNormal && second is NormalPuzzleObject secondNormal)
        {
            if (currentStep.playHintAnimation)
                swapHint.Play(firstNormal, secondNormal, swapHintBrightness, swapHintScaleAmount,
                    swapHintPulseDuration, swapHintPulseGap, swapHintRepeatDelay, swapHintGlowColor);
            ShowSwapHintCircle();
        }
    }

    private void ShowSwapHintCircle()
    {
        // 매치 줄 밖에서 교환을 시작하는 타일을 안내합니다.
        if (!currentStep.showHintCircle || hintCirclePrefab == null || highlightMask == null)
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
        if (!IsSwapHintWaiting || puzzle == null)
            return;

        Vector2Int position = new Vector2Int(puzzle.column, puzzle.row);
        bool isHintTile = position == currentStep.from ||
            (currentStep.waitType == WaitType.Swap && position == currentStep.to);
        if (!isHintTile)
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
        SetReplayButtonVisible(false);
        swapHint?.Stop();
        DestroyHintCircle();
        hintPressedTile = null;
        waitingForPuzzleAction = false;
        puzzleActionRunning = true;
        puzzleGenerator.SetTutorialInputRestriction(true);
        if (currentStep.recordSpawnedUnits)
            recordedSpawnedUnits.Clear();
        if (currentStep.actionFriendlyGrade >= 0)
            friendlyGradeOverride = currentStep.actionFriendlyGrade;
        // 매치와 에너지 이동은 별도 퍼즐 시간으로 진행하며 전투는 정지 상태를 유지합니다.
        currentStep.onActionStarted?.Invoke();
    }

    private void HandleFriendlySpawning()
    {
        if (!puzzleActionRunning || currentStep == null || !currentStep.hidePanelBeforeSpawn)
            return;

        SetPanel(tutorialPanel, false, false);
        SetPanel(underTutorialPanel, false, false);
        highlightMask?.RequestRefresh();
    }

    private void HandleFriendlySpawned(FriendlyUnit unit)
    {
        if (unit != null && HasActiveBattle && mirrorEnemySpawns)
        {
            // 실제 아군 한 마리 소환마다 같은 프레임에 Farmer(0) 또는 Axe(2)를 한 마리 소환합니다.
            int enemyIndex = tutorialEnemyIndices[UnityEngine.Random.Range(0, tutorialEnemyIndices.Length)];
            friendlySpawner.SpawnTutorialEnemy(enemyIndex);
        }

        if (currentStep != null && currentStep.recordSpawnedUnits && unit != null)
            recordedSpawnedUnits.Add(unit);

        if (puzzleActionRunning)
            SetTutorialPaused(false);
    }

    private void HandleSuccessfulSwap(Vector2Int from, Vector2Int to)
    {
        if (!IsExpectedSwap(from, to))
            return;

        actionCompleted = true;
        ApplyAfterActionSettings();
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

        BeginPuzzleAction();
    }

    private void HandleSpecialTile(Vector2Int position, SpecialPuzzleType type)
    {
        if (!IsExpectedSpecialTile(position, type))
            return;

        actionCompleted = true;
        ApplyAfterActionSettings();
        puzzleGenerator.SetTutorialInputRestriction(true);
    }

    private void ApplyAfterActionSettings()
    {
        if (currentStep.afterActionFriendlyGrade >= 0)
            friendlyGradeOverride = currentStep.afterActionFriendlyGrade;
    }

    private bool ValidateSetup()
    {
        if (tutorialText == null || tutorialPanel == null || steps == null || steps.Length == 0)
        {
            Debug.LogError("TutorialManager: 대사 UI, 패널 및 단계를 확인해 주세요.", this);
            return false;
        }
        if (useBattleRules && (puzzleGenerator == null || GameManager.Instance == null ||
            !GameManager.Instance.Istutorial || StageManager.Instance == null))
        {
            Debug.LogError("TutorialManager: 전투 규칙에는 튜토리얼 모드, PuzzleGenerator와 StageManager가 필요합니다.", this);
            return false;
        }
        if (useBattleRules && mirrorEnemySpawns &&
            (tutorialEnemyIndices == null || tutorialEnemyIndices.Length == 0 ||
             Array.Exists(tutorialEnemyIndices, index => index < 0)))
        {
            Debug.LogError("TutorialManager: 적 소환 인덱스를 하나 이상 지정해 주세요.", this);
            return false;
        }
        if (useBattleRules && mirrorEnemySpawns && puzzleGenerator.FriendlySpawner == null)
        {
            Debug.LogError("TutorialManager: 아군/적 동시 소환에 사용할 UnitSpawner가 필요합니다.", this);
            return false;
        }

        foreach (TutorialStep step in steps)
        {
            if (step == null || (string.IsNullOrWhiteSpace(step.dialogueKey) &&
                (tutorialTextList == null || step.dialogueIndex < 0 || step.dialogueIndex >= tutorialTextList.Count)))
            {
                Debug.LogError("TutorialManager: 단계에 대사 키 또는 유효한 대사 목록 인덱스를 지정해 주세요.", this);
                return false;
            }
            if (!useBattleRules && (IsPuzzleActionStep(step) || step.enterAction == StepAction.CreateObstacle ||
                step.focusSpawnedUnit || step.recordSpawnedUnits))
            {
                Debug.LogError("TutorialManager: 퍼즐/소환 단계에는 Use Battle Rules를 켜 주세요.", this);
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
        StopTutorial();
    }

    private void CleanUpSession()
    {
        RestorePopupPresentation();
        ReleaseActionButton();
        SetReplayButtonVisible(false);
        if (tutorialTextReplayButton != null)
            tutorialTextReplayButton.onClick.RemoveListener(ReplayExplanation);
        replayRequested = false;
        isReplaying = false;
        RestoreHighlightedUnitSortingOrders();
        battleCameraController?.EndTutorialFocus();
        swapHint?.Stop();
        DestroyHintCircle();
        hintPressedTile = null;
        waitingForDialogue = false;
        waitingForPuzzleAction = false;
        puzzleActionRunning = false;

        recordedSpawnedUnits.Clear();

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
        currentStep = null;
        SetPanel(tutorialPanel, false, false);
        SetPanel(underTutorialPanel, false, false);
        if (tutorialEndButton != null)
            tutorialEndButton.SetActive(false);
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextDialogue);
        if (underNextButton != null)
            underNextButton.onClick.RemoveListener(NextDialogue);
        if (useBattleRules && puzzleGenerator != null)
        {
            puzzleGenerator.OnSwapStarted -= HandleSwapStarted;
            puzzleGenerator.OnPuzzlePointerDown -= HandlePuzzlePointerDown;
            puzzleGenerator.OnPuzzlePointerUp -= HandlePuzzlePointerUp;
            puzzleGenerator.OnSuccessfulSwapResolved -= HandleSuccessfulSwap;
            puzzleGenerator.OnSpecialTileResolved -= HandleSpecialTile;
            puzzleGenerator.OnSpecialTileStarted -= HandleSpecialTileStarted;
        }
        if (friendlySpawner != null)
        {
            friendlySpawner.OnFriendlySpawning -= HandleFriendlySpawning;
            friendlySpawner.OnFriendlySpawned -= HandleFriendlySpawned;
        }
        ReleaseInputRestriction();
        RestoreTutorialTimeScale();
        RestoreSessionSettings();
        friendlySpawner = null;
        sessionPrepared = false;
        if (activeTutorial == this)
            activeTutorial = null;
    }

    private void OnDestroy()
    {
        // OnDisable 이후에도 안전하게 호출할 수 있습니다.
        StopTutorial();
        if (screenNextInput != null)
            Destroy(screenNextInput.gameObject);
    }

    private void RestoreSessionSettings()
    {
        if (stageApplied && stageManager != null)
            stageManager.SetStage(previousStage);
        if (habitatApplied && habitatModeManager != null)
            habitatModeManager.IsHabitatBattle = previousHabitatBattle;
        stageApplied = false;
        habitatApplied = false;
        foreach (var pair in savedUiStates)
        {
            if (pair.Key == null)
                continue;
            UiState state = pair.Value;
            pair.Key.alpha = state.alpha;
            pair.Key.interactable = state.interactable;
            pair.Key.blocksRaycasts = state.blocksRaycasts;
            pair.Key.gameObject.SetActive(state.active);
        }
        savedUiStates.Clear();
    }

    private void SaveUiState(CanvasGroup group)
    {
        if (group == null || savedUiStates.ContainsKey(group))
            return;
        savedUiStates.Add(group, new UiState
        {
            active = group.gameObject.activeSelf,
            alpha = group.alpha,
            interactable = group.interactable,
            blocksRaycasts = group.blocksRaycasts
        });
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
