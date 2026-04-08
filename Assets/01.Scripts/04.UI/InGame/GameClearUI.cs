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
            GameManager.Instance.playData.clearedStage = 
                Mathf.Max(GameManager.Instance.playData.clearedStage, StageManager.Instance.CurrentStage);
            StageManager.Instance.SetMaxStage(GameManager.Instance.playData.clearedStage + 1);
            
            StageManager.Instance.StopStage();
            float time = StageManager.Instance.CurrentTime;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            
            GoldManager.Instance.AddStageClearedGold();
            
            Time.timeScale = 0.1f;
            DepthOfField.active = true;
            stageText.text = $"지켜낸 서식지 {StageManager.Instance.CurrentStage + 1}";
            timeText.text = $"{minutes}:{seconds:00}";
            coinText.text = $"{GoldManager.Instance.Gold}";
        }
    }
}
