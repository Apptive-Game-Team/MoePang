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
        [Header("Text Setting")]
        [SerializeField] private TextMeshProUGUI modeTitleText;
        [SerializeField] private TextMeshProUGUI modeDescriptionText;
        [SerializeField] private TextMeshProUGUI clearRewardText;
        [SerializeField] private TextMeshProUGUI currentBonusTitle;
        [SerializeField] private TextMeshProUGUI currentBonusText;
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

            if (clearRewardText != null)
            {
                clearRewardText.text = GetModeClearRewardText(mode);
            }

            if (currentBonusTitle != null)
            {
                currentBonusTitle.text = GetModeCurrentBonusTitle(mode);
            }

            if (currentBonusText != null)
            {
                currentBonusText.text = GetModeCurrentBonusText(mode);
            }
        }

        private string GetModeTitleText(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => "Meadow",
                HabitatMode.OceanMode => "Ocean",
                HabitatMode.DesertMode => "Desert",
                HabitatMode.ForestMode => "Forest",
                HabitatMode.PolarMode => "Polar",
                _ => mode.ToString()
            };
        }

        private string GetModeDescriptionText(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode =>
                    "Summoning units requires more stacks\n" +
                    "<size=30>Stack Cost : 3 -> 6</size>",

                HabitatMode.OceanMode =>
                    "Enemies gain increased Stats\n" +
                    "<size=30>All stats × 1.5</size>",

                HabitatMode.DesertMode =>
                    "A sandstorm periodically obscures the puzzle board\n" +
                    "<size=30>Every 15s, tiles are hidden for 3s.</size>",

                HabitatMode.ForestMode =>
                    "Friendly units have reduced Movement Speed and Attack Speed\n" +
                    "<size=30>Speed & Attack Stats × 0.75</size>",

                HabitatMode.PolarMode =>
                    "Enemies periodically recover HP\n" +
                    "<size=30>All enemies restore HP every 15s</size>",

                _ => mode.ToString()
            };
        }

        private string GetModeClearRewardText(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => "Meadow Reward :\n" + "Hp + 999 / Attack + 999\n" + "Dia + 999",
                HabitatMode.OceanMode => "Ocean Reward :\n" + "Hp + 999 / Attack + 999\n" + "Dia + 999",
                HabitatMode.DesertMode => "Desert Reward :\n" + "Hp + 999 / Attack + 999\n" + "Dia + 999",
                HabitatMode.ForestMode => "Forest Reward :\n" + "Hp + 999 / Attack + 999\n" + "Dia + 999",
                HabitatMode.PolarMode => "Polar Reward :\n" + "Hp + 999 / Attack + 999\n" + "Dia + 999",
                _ => mode.ToString()
            };
        }

        private string GetModeCurrentBonusTitle(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => "Current Meadow Bonus",
                HabitatMode.OceanMode => "Current Ocean Bonus",
                HabitatMode.DesertMode => "Current Desert Bonus",
                HabitatMode.ForestMode => "Current Forest Bonus",
                HabitatMode.PolarMode => "Current Polar Bonus",
                _ => mode.ToString()
            };
        }

        private string GetModeCurrentBonusText(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => "In Developing...",
                HabitatMode.OceanMode => "In Developing...",
                HabitatMode.DesertMode => "In Developing...",
                HabitatMode.ForestMode => "In Developing...",
                HabitatMode.PolarMode => "In Developing...",
                _ => mode.ToString()
            };
        }
    } 
}
