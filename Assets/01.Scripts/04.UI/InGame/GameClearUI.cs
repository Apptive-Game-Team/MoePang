using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    public class GameClearUI : GameUI
    {
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI coinText;

        protected override void OnEnable()
        {
            // Save
            StageManager stageManager = StageManager.Instance;
            GoldManager goldManager = GoldManager.Instance;
            float time = stageManager.CurrentTime;
            
            stageManager.StopStage();
            goldManager.AddStageClearedGold();
            GameManager.Instance.SavePlayData();
            
            // UI
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            
            Time.timeScale = 0f;
            DepthOfField.active = true;
            stageText.text = $"지켜낸 서식지 {stageManager.CurrentStage + 1}";
            timeText.text = $"{minutes}:{seconds:00}";
            coinText.text = $"{goldManager.Gold}";
        }
    }
}
