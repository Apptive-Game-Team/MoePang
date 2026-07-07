using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;
using _01.Scripts._11.HabitatMode;

namespace _01.Scripts._04.UI.InGame
{
    public class GameClearUI : GameUI
    {
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private GameObject diaInfo;
        [SerializeField] private TextMeshProUGUI diaText;

        protected override void OnEnable()
        {
            DepthOfField.active = true;
            
            // Save
            StageManager stageManager = StageManager.Instance;
            GoldManager goldManager = GoldManager.Instance;
            float time = stageManager.CurrentTime;
            
            stageManager.StopStage();
            goldManager.AddStageClearedGold();
            
            int diaAmount = 0;
            bool isHabitatBattle =
                HabitatModeManager.Instance != null &&
                HabitatModeManager.Instance.IsHabitatBattle;

            if (isHabitatBattle)
            {
                diaAmount = goldManager.AddStageClearedDia();
            }
            
            GameManager.Instance.SavePlayData();
            
            // UI
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            
            Time.timeScale = 0f;
            DepthOfField.active = true;
            if (HabitatModeManager.Instance != null &&
                HabitatModeManager.Instance.IsHabitatBattle)
            {
                HabitatMode mode = HabitatModeManager.Instance.HabitatMode;
                string habitatName = GetHabitatName(mode);
                int habitatStage = stageManager.GetHabitatStage(mode) + 1;

                stageText.text = $"지켜낸 {habitatName} 서식지 {habitatStage}";
            }
            else
            {
                stageText.text = $"지켜낸 서식지 {stageManager.CurrentStage + 1}";
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
    }
}
