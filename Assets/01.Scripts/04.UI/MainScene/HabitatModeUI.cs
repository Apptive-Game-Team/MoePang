using _01.Scripts._08.Utility;
using _01.Scripts._11.HabitatMode;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._04.UI.MainScene
{
    public class HabitatModeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI modeTitleText;
        [SerializeField] private TextMeshProUGUI modeDescriptionText;
        [SerializeField] private SceneType startScene = SceneType.HabitatBattle;

        private HabitatMode selectedMode = HabitatMode.MeadowMode;

        private void OnEnable()
        {
            SelectMode(HabitatMode.MeadowMode);
        }

        public void SelectMode(int modeIndex)
        {
            if (!Enum.IsDefined(typeof(HabitatMode), modeIndex))
            {
                Debug.LogError($"Invalid habitat mode index: {modeIndex}");
                return;
            }

            SelectMode((HabitatMode)modeIndex);
        }

        public void StartMode()
        {
            HabitatModeManager.Instance.HabitatMode = selectedMode;
            SceneManager.LoadScene(SceneInfo.GetSceneName(startScene));
        }

        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }

        private void SelectMode(HabitatMode mode)
        {
            selectedMode = mode;
            HabitatModeManager.Instance.HabitatMode = mode;

            if (modeTitleText != null)
            {
                modeTitleText.text = GetModeTitleText(mode);
            }

            if (modeDescriptionText != null)
            {
                modeDescriptionText.text = GetModeDescriptionText(mode);
            }
        }

        private string GetModeTitleText(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => "Meadow Mode",
                HabitatMode.OceanMode => "Ocean Mode",
                HabitatMode.DesertMode => "Desert Mode",
                HabitatMode.ForestMode => "Forest Mode",
                HabitatMode.PolarMode => "Polar Mode",
                _ => mode.ToString()
            };
        }

        private string GetModeDescriptionText(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => "Meadow Mode",
                HabitatMode.OceanMode => "Ocean Mode",
                HabitatMode.DesertMode => "Desert Mode",
                HabitatMode.ForestMode => "Forest Mode",
                HabitatMode.PolarMode => "Polar Mode",
                _ => mode.ToString()
            };
        }
    } 
}
