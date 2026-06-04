using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using _01.Scripts._08.Utility;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._11.HabitatMode
{
    public class HabitatModeManager : SingletonObject<HabitatModeManager>
    {
        [SerializeField] private HabitatMode habitatMode = HabitatMode.MeadowMode;
        
        [Header("Ocean Debuff")]
        [SerializeField] private float oceanEnemyStatMultiplier = 1.5f;
        [SerializeField] private float oceanEventEnemyStatMultiplier = 1.25f;
        [SerializeField] private float oceanEnemyBuffDuration = 9999f;
        
        [Header("Forest Debuff")]
        [SerializeField] private float forestEnemyHealAmount = 15f;
        [SerializeField] private float forestEnemyHealInterval = 15f;
        [SerializeField] private float forestEventEnemyHealInterval = 25f;
        
        [Header("Polar Debuff")]
        [SerializeField] private float polarFriendlyStatMultiplier = 0.75f;
        [SerializeField] private float polarEventFriendlyStatMultiplier = 0.85f;
        [SerializeField] private float polarFriendlyBuffDuration = 9999f;

        private SpawnStackManager spawnStackManager;
        
        public event Action<HabitatMode> HabitatModeApplied;

        public HabitatMode HabitatMode
        {
            get => habitatMode;
            set => habitatMode = value;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != SceneInfo.GetSceneName(SceneType.HabitatBattle))
            {
                return;
            }
            
            spawnStackManager = FindFirstObjectByType<SpawnStackManager>();

            ApplyHabitatModeEffect();
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

        private void ApplyMeadowEffect()
        {
            spawnStackManager.SetAllStackMaxCount(6);
            
            Debug.Log("Apply Meadow Mode effect.");
        }

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

        private void ApplyDesertEffect()
        {
            Debug.Log("Apply Desert Mode effect.");
        }

        private void ApplyForestEffect()
        {
            float healInterval = IsHabitatModeEventDay(HabitatMode.ForestMode)
                ? forestEventEnemyHealInterval
                : forestEnemyHealInterval;

            BuffManager.Instance.StartEnemyHealOverTime(forestEnemyHealAmount, healInterval);

            Debug.Log("Apply Forest Mode effect.");
        }

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
    }
}
