using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using _01.Scripts._11.HabitatMode;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.InGame
{
    public class GameClearUI : GameUI
    {
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private GameObject diaInfo;
        [SerializeField] private TextMeshProUGUI diaText;
        [SerializeField] private Image comboUnlockedPopup;
        [SerializeField] private Sprite[] habitatSprites;

        protected override void OnEnable()
        {
            DepthOfField.active = true;
            SoundManager.Instance.PlaySFX(SFX.SFX14_StageClear);
            
            // Save
            GameManager gameManager = GameManager.Instance;
            StageManager stageManager = StageManager.Instance;
            GoldManager goldManager = GoldManager.Instance;
            HabitatModeManager habitatModeManager = HabitatModeManager.Instance;
            float time = stageManager.CurrentTime;
            
            stageManager.StopStage();
            goldManager.AddStageClearedGold();
            
            int diaAmount = 0;
            bool isHabitatBattle =
                habitatModeManager != null &&
                habitatModeManager.IsHabitatBattle;

            if (isHabitatBattle)
            {
                diaAmount = goldManager.AddStageClearedDia();
                SetComboUnlockedPopup();
            }
            
            // UI
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            
            Time.timeScale = 0f;
            DepthOfField.active = true;
            string protectedText = LocalizationSettings.StringDatabase.GetLocalizedString("Localization", "GameClearUI_Protected");
            string habitatText = LocalizationSettings.StringDatabase.GetLocalizedString("Localization", "GameClearUI_Habitat");

            if (isHabitatBattle)
            {
                HabitatMode mode = habitatModeManager.HabitatMode;
                string habitatName = GetHabitatName(mode);
                int habitatStage = stageManager.GetHabitatStage(mode) + 1;

                stageText.text = $"{protectedText} {habitatName} {habitatText} {habitatStage}";
            }
            else
            {
                stageText.text = $"{protectedText} {habitatText} {stageManager.CurrentStage + 1}";
            }
            timeText.text = $"{minutes}:{seconds:00}";
            coinText.text = $"{goldManager.Gold}";
            if (diaInfo != null)
            {
                diaInfo.SetActive(isHabitatBattle);
            }

            if (diaText != null)
            {
                diaText.text = $"{diaAmount}";
            }
            
            gameManager.SavePlayData();
        }
        
        private string GetHabitatName(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => "초원",
                HabitatMode.OceanMode => "바다",
                HabitatMode.DesertMode => "사막",
                HabitatMode.ForestMode => "숲",
                HabitatMode.PolarMode => "극지",
                _ => mode.ToString()
            };
        }

        private void SetComboUnlockedPopup()
        {
            HabitatMode mode = HabitatModeManager.Instance.HabitatMode;
            StageType type = GameManager.Instance.GetStageTypeWithHabitat(mode);
            Image habitatImage = comboUnlockedPopup.transform.GetChild(1).GetComponent<Image>();

            if (GameManager.Instance.playData.MaxStages[type] == 4 &&
                StageManager.Instance.GetHabitatStage(mode) == 4)
            {
                habitatImage.sprite = habitatSprites[(int)mode];
                
                Button button = comboUnlockedPopup.GetComponentInChildren<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    comboUnlockedPopup.gameObject.SetActive(false);
                });
                
                comboUnlockedPopup.gameObject.SetActive(true);
            }
        }
    }
}
