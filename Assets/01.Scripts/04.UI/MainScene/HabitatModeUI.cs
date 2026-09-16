using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using _01.Scripts._11.HabitatMode;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

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

        [Header("Info Card Setting")]
        [SerializeField] private GameObject guidePanel;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Stage Card Setting")]
        [SerializeField] private GameObject previousStageButton;
        [SerializeField] private GameObject nextStageButton;
        [SerializeField] private TextMeshProUGUI stageText;

        private HabitatMode selectedMode = HabitatMode.MeadowMode;

        private void OnEnable()
        {
            HabitatMode mode = HabitatMode.MeadowMode;
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
            
            StageManager.Instance.SetHabitatStage(
                selectedMode,
                StageManager.Instance.GetHabitatStage(selectedMode)
            );
            
            RefreshStageUI();
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
            if (StageManager.Instance.MaxStage <= 50)
            {
                Debug.Log("아직 50스테이지 안깼어, 근데 일단은 실행됨");
            }
            
            // fix : Onclick 매서드로 분리하여 사운드 책임 분할 요망
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
            HabitatModeManager.Instance.IsHabitatBattle = true;
            HabitatModeManager.Instance.HabitatMode = selectedMode;
            
            
            StageManager.Instance.SetHabitatStage(
                selectedMode,
                StageManager.Instance.GetHabitatStage(selectedMode)
            );

            SceneManager.sceneLoaded += OnHabitatPlaySceneLoaded;
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
        }
        
        private void OnHabitatPlaySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StageManager.Instance.StartStage();
            GameManager.Instance.PlayBattleBGM();
            SceneManager.sceneLoaded -= OnHabitatPlaySceneLoaded;
        }

        public void ClosePanel()
        {
            // fix : Onclick 매서드로 분리하여 사운드 책임 분할 요망
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
            gameObject.SetActive(false);
        }

        private void SelectMode(HabitatMode mode)
        {
            // fix : Onclick 매서드로 분리하여 사운드 책임 분할 요망
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
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
            
            StageManager.Instance.SetHabitatStage(
                selectedMode,
                StageManager.Instance.GetHabitatStage(selectedMode)
            );
            
            RefreshStageUI();
        }
        
        public void OnClickNextStage()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
            StageManager.Instance.AddHabitatStage(selectedMode, 1);
            RefreshStageUI();
        }

        public void OnClickPrevStage()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
            StageManager.Instance.AddHabitatStage(selectedMode, -1);
            RefreshStageUI();
        }

        private void RefreshStageUI()
        {
            int currentStage = StageManager.Instance.GetHabitatStage(selectedMode);
            int maxStage = StageManager.Instance.GetMaxHabitatStage(selectedMode);
            Debug.Log($"서식지모드 UI : currentStage : {currentStage}, maxStage : {maxStage}");

            if (stageText != null)
            {
                string stageLabel = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Stage");
                stageText.text = $"{stageLabel} : {currentStage + 1}";
            }

            if (previousStageButton != null)
            {
                previousStageButton.SetActive(currentStage > 0);
            }

            if (nextStageButton != null)
            {
                nextStageButton.SetActive(currentStage < maxStage);
            }
        }

        private string GetModeTitleText(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Habitat_Meadow"),
                HabitatMode.OceanMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Habitat_Ocean"),
                HabitatMode.DesertMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Habitat_Desert"),
                HabitatMode.ForestMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Habitat_Forest"),
                HabitatMode.PolarMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Habitat_Polar"),
                _ => mode.ToString()
            };
        }

        private string GetModeDescriptionText(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "06MeadowDescription"),
                HabitatMode.OceanMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "06OceanDescription"),
                HabitatMode.DesertMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "06DesertDescription"),
                HabitatMode.ForestMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "06ForestDescription"),
                HabitatMode.PolarMode => LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "06PolarDescription"),
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

        public void ShowGuide()
        {
            // fix : Onclick 매서드로 분리하여 사운드 책임 분할 요망
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
            if (guidePanel != null)
            {
                guidePanel.SetActive(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        public void HideGuide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (guidePanel != null)
            {
                guidePanel.SetActive(false);
            }
        }

    } 
}
