using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using _01.Scripts._08.Utility;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace _01.Scripts._11.HabitatMode
{
    /// <summary>
    /// 서식지모드 담당 스크립트
    /// <para>1. 요일별 버프 / 디버프 관리</para>
    /// </summary>
    public class HabitatModeManager : SingletonObject<HabitatModeManager>
    {
        public HabitatMode habitatMode = HabitatMode.MeadowMode;

        [Header("Debug")]
        public bool DebugHabitat = false;
        
        [Header("Ocean Debuff")]
        [SerializeField] private float oceanEnemyStatMultiplier = 1.5f;
        [SerializeField] private float oceanEventEnemyStatMultiplier = 1.25f;
        [SerializeField] private float oceanEnemyBuffDuration = 9999f;
        
        [Header("Desert Debuff")]
        [SerializeField] private GameObject desertPuzzleCoverPrefab;
        [SerializeField] private GameObject desertSandStormPrefab;
        [SerializeField] private GameObject desertSandParticlePrefab;
        [SerializeField] private float desertPuzzleCoverInterval = 15f;
        [SerializeField] private float desertEventPuzzleCoverInterval = 25f;
        [SerializeField] private float desertPuzzleCoverDuration = 5f;
        [SerializeField] private float desertPuzzleCoverRefreshInterval = 0.1f;
        
        [Header("Forest Debuff")]
        [SerializeField] private float forestEnemyHealAmount = 15f;
        [SerializeField] private float forestEnemyHealInterval = 15f;
        [SerializeField] private float forestEventEnemyHealInterval = 25f;
        
        [Header("Polar Debuff")]
        [SerializeField] private float polarFriendlyStatMultiplier = 0.75f;
        [SerializeField] private float polarEventFriendlyStatMultiplier = 0.85f;
        [SerializeField] private float polarFriendlyBuffDuration = 9999f;

        private bool isHabitatBattle = false;
        private SpawnStackManager spawnStackManager;
        private PuzzleGenerator puzzleGenerator;
        private Coroutine desertPuzzleCoverCoroutine;

        public bool IsHabitatBattle
        {
            get => isHabitatBattle;
            set => isHabitatBattle = value;
        }
        
        public event Action<HabitatMode> HabitatModeApplied;

        public HabitatMode HabitatMode
        {
            get => habitatMode;
            set => habitatMode = value;
        }

        private void OnEnable()
        {
            isHabitatBattle = false;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            ClearTestDebuff();
            testDebuffMode = null;
            StopHabitatModeEffects();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ClearTestDebuff();
            testDebuffMode = null;
            if (scene.name != SceneInfo.GetSceneName(SceneType.MatchAndBattle))
            {
                return;
            }

            spawnStackManager = FindFirstObjectByType<SpawnStackManager>();
            puzzleGenerator = FindFirstObjectByType<PuzzleGenerator>();
            
            if (!isHabitatBattle)
            {
                StopHabitatModeEffects();
                return;
            }
            
            Debug.Log("Habitat Battle Start!");

            ApplyHabitatModeEffect();
        }
        private void StopHabitatModeEffects()
        {
            if (desertPuzzleCoverCoroutine != null)
            {
                StopCoroutine(desertPuzzleCoverCoroutine);
                desertPuzzleCoverCoroutine = null;
            }

            if (BuffManager.Instance != null)
            {
                BuffManager.Instance.StopEnemyHealOverTime();
            }
        }
        

        public void ApplyHabitatModeEffect()
        {
            switch (habitatMode)
            {
                case HabitatMode.MeadowMode:
                    ApplyMeadowEffect();
                    break;
                case HabitatMode.OceanMode:
                    ApplyOceanEffect();
                    break;
                case HabitatMode.DesertMode:
                    ApplyDesertEffect();
                    break;
                case HabitatMode.ForestMode:
                    ApplyForestEffect();
                    break;
                case HabitatMode.PolarMode:
                    ApplyPolarEffect();
                    break;
            }

            HabitatModeApplied?.Invoke(habitatMode);
        }

        public bool IsHabitatModeEventDay(HabitatMode mode)
        {
            DayOfWeek koreaDay = GetKoreaDayOfWeek();

            if (koreaDay == DayOfWeek.Saturday || koreaDay == DayOfWeek.Sunday)
            {
                return true;
            }

            switch (koreaDay)
            {
                case DayOfWeek.Monday:
                    return mode == HabitatMode.MeadowMode;
                case DayOfWeek.Tuesday:
                    return mode == HabitatMode.OceanMode;
                case DayOfWeek.Wednesday:
                    return mode == HabitatMode.DesertMode;
                case DayOfWeek.Thursday:
                    return mode == HabitatMode.ForestMode;
                case DayOfWeek.Friday:
                    return mode == HabitatMode.PolarMode;
                default:
                    return false;
            }
        }

        private DayOfWeek GetKoreaDayOfWeek()
        {
            return DateTime.UtcNow.AddHours(9).DayOfWeek;
        }

        /// <summary>
        /// 초원 모드 버프
        /// <para>Stack 갯수 변경 : 5 or 6</para>
        /// </summary>
        private void ApplyMeadowEffect()
        {
            int stackMaxCount = IsHabitatModeEventDay(HabitatMode.MeadowMode) ? 5 : 6;
            spawnStackManager.SetAllStackMaxCount(stackMaxCount);
            
            Debug.Log("Apply Meadow Mode effect.");
        }

        /// <summary>
        /// 바다 모드 버프
        /// <para>적 능력치 상승</para>
        /// </summary>
        private void ApplyOceanEffect()
        {
            float statMultiplier = IsHabitatModeEventDay(HabitatMode.OceanMode)
                ? oceanEventEnemyStatMultiplier
                : oceanEnemyStatMultiplier;

            BuffManager.Instance.ApplyEnemyBuff(
                StatType.AttackSpeed,
                statMultiplier,
                oceanEnemyBuffDuration
            );

            BuffManager.Instance.ApplyEnemyBuff(
                StatType.AttackDamage,
                statMultiplier,
                oceanEnemyBuffDuration
            );

            BuffManager.Instance.ApplyEnemyBuff(
                StatType.MoveSpeed,
                statMultiplier,
                oceanEnemyBuffDuration
            );

            Debug.Log("Apply Ocean Mode effect.");
        }

        /// <summary>
        /// 사막 모드 버프
        /// <para>일정시간마다 모래바람이 발생</para>
        /// </summary>
        private void ApplyDesertEffect()
        {
            if (desertPuzzleCoverCoroutine != null)
            {
                StopCoroutine(desertPuzzleCoverCoroutine);
            }

            desertPuzzleCoverCoroutine = StartCoroutine(DesertPuzzleCoverRoutine());

            Debug.Log("Apply Desert Mode effect.");
        }
        
        private IEnumerator DesertPuzzleCoverRoutine(bool useNormalValues = false)
        {
            while (true)
            {
                float coverInterval = useNormalValues ? 15f : IsHabitatModeEventDay(HabitatMode.DesertMode)
                    ? desertEventPuzzleCoverInterval
                    : desertPuzzleCoverInterval;

                yield return new WaitForSeconds(coverInterval);

                if (puzzleGenerator == null)
                {
                    puzzleGenerator = FindFirstObjectByType<PuzzleGenerator>();
                }

                if (puzzleGenerator != null)
                {
                    StartCoroutine(puzzleGenerator.StartDesertPuzzleCover(
                        desertPuzzleCoverPrefab, desertSandStormPrefab, desertSandParticlePrefab,
                        desertPuzzleCoverDuration,
                        desertPuzzleCoverRefreshInterval
                    ));
                }
            }
        }

        /// <summary>
        /// 숲 모드 버프
        /// <para>일정 주기로 적이 회복</para>
        /// </summary>
        private void ApplyForestEffect()
        {
            float healInterval = IsHabitatModeEventDay(HabitatMode.ForestMode)
                ? forestEventEnemyHealInterval
                : forestEnemyHealInterval;

            BuffManager.Instance.StartEnemyHealOverTime(forestEnemyHealAmount, healInterval);

            Debug.Log("Apply Forest Mode effect.");
        }

        /// <summary>
        /// 극지방 모드 버프
        /// <para>아군 수치 하향</para>
        /// </summary>
        private void ApplyPolarEffect()
        {
            float statMultiplier = IsHabitatModeEventDay(HabitatMode.PolarMode)
                ? polarEventFriendlyStatMultiplier
                : polarFriendlyStatMultiplier;

            BuffManager.Instance.ApplyAllyBuff(
                StatType.MoveSpeed,
                statMultiplier,
                polarFriendlyBuffDuration
            );

            BuffManager.Instance.ApplyAllyBuff(
                StatType.AttackSpeed,
                statMultiplier,
                polarFriendlyBuffDuration
            );
            
            Debug.Log("Apply Polar Mode effect.");
        }
        // 테스트 디버프 순환: Meadow -> Ocean -> Desert -> Forest -> Polar -> 없음
        private HabitatMode? testDebuffMode;
        private readonly List<Buff> testBuffs = new();
        private BuffManager testBuffManager;
        private Coroutine testDesertCoroutine;
        private Action restoreTestStacks;

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.isTestMode)
            {
                if (testDebuffMode.HasValue)
                {
                    ClearTestDebuff();
                    testDebuffMode = null;
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                CycleTestDebuff();
            }
        }

        private void CycleTestDebuff()
        {
            spawnStackManager = FindFirstObjectByType<SpawnStackManager>();
            puzzleGenerator = FindFirstObjectByType<PuzzleGenerator>();
            if (spawnStackManager == null || puzzleGenerator == null || BuffManager.Instance == null)
            {
                Debug.LogWarning("[Habitat Test] 전투 씬에서만 디버프를 변경할 수 있습니다.");
                return;
            }

            ClearTestDebuff();
            switch (testDebuffMode)
            {
                case null:
                    testDebuffMode = HabitatMode.MeadowMode;
                    break;
                case HabitatMode.MeadowMode:
                    testDebuffMode = HabitatMode.OceanMode;
                    break;
                case HabitatMode.OceanMode:
                    testDebuffMode = HabitatMode.DesertMode;
                    break;
                case HabitatMode.DesertMode:
                    testDebuffMode = HabitatMode.ForestMode;
                    break;
                case HabitatMode.ForestMode:
                    testDebuffMode = HabitatMode.PolarMode;
                    break;
                case HabitatMode.PolarMode:
                    testDebuffMode = null;
                    break;
            }

            testBuffManager = BuffManager.Instance;
            switch (testDebuffMode)
            {
                case HabitatMode.MeadowMode:
                    restoreTestStacks = spawnStackManager.CaptureStackMaxCounts();
                    spawnStackManager.SetAllStackMaxCount(6);
                    break;
                case HabitatMode.OceanMode:
                    testBuffs.Add(testBuffManager.ApplyEnemyBuff(StatType.AttackSpeed, 1.5f, float.PositiveInfinity));
                    testBuffs.Add(testBuffManager.ApplyEnemyBuff(StatType.AttackDamage, 1.5f, float.PositiveInfinity));
                    testBuffs.Add(testBuffManager.ApplyEnemyBuff(StatType.MoveSpeed, 1.5f, float.PositiveInfinity));
                    break;
                case HabitatMode.DesertMode:
                    testDesertCoroutine = StartCoroutine(DesertPuzzleCoverRoutine(true));
                    break;
                case HabitatMode.ForestMode:
                    testBuffManager.StartEnemyHealOverTime(15f, 15f);
                    break;
                case HabitatMode.PolarMode:
                    testBuffs.Add(testBuffManager.ApplyAllyBuff(StatType.MoveSpeed, 0.75f, float.PositiveInfinity));
                    testBuffs.Add(testBuffManager.ApplyAllyBuff(StatType.AttackSpeed, 0.75f, float.PositiveInfinity));
                    break;
            }
            Debug.Log($"[Habitat Test] Debuff: {testDebuffMode?.ToString() ?? "None"}");
        }

        private void ClearTestDebuff()
        {
            if (testDesertCoroutine != null)
            {
                // 이미 진행 중인 가림 연출은 끝까지 재생하고 다음 발생만 중단한다.
                StopCoroutine(testDesertCoroutine);
                testDesertCoroutine = null;
            }

            if (testBuffManager != null)
            {
                if (testDebuffMode == HabitatMode.ForestMode)
                    testBuffManager.StopEnemyHealOverTime();

                foreach (Buff buff in testBuffs)
                    testBuffManager.RemoveBuff(buff);
            }
            testBuffs.Clear();
            testBuffManager = null;
            restoreTestStacks?.Invoke();
            restoreTestStacks = null;
        }
    }
}
