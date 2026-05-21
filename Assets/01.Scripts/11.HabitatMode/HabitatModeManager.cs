using _01.Scripts._08.Utility;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._11.HabitatMode
{
    public class HabitatModeManager : SingletonObject<HabitatModeManager>
    {
        [SerializeField] private HabitatMode habitatMode = HabitatMode.MeadowMode;

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

        private void ApplyMeadowEffect()
        {
            Debug.Log("Apply Meadow Mode effect.");
        }

        private void ApplyOceanEffect()
        {
            Debug.Log("Apply Ocean Mode effect.");
        }

        private void ApplyDesertEffect()
        {
            Debug.Log("Apply Desert Mode effect.");
        }

        private void ApplyForestEffect()
        {
            Debug.Log("Apply Forest Mode effect.");
        }

        private void ApplyPolarEffect()
        {
            Debug.Log("Apply Polar Mode effect.");
        }
    }
}

