using TMPro;
using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    public class GameOverUI : GameUI
    {
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI coinText;
        
        protected override void OnEnable()
        {
            StageManager.Instance.StopStage();
            float time = StageManager.Instance.CurrentTime;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            
            Time.timeScale = 0.1f;
            timeText.text = $"{minutes}:{seconds:00}";
            coinText.text = $"{GoldManager.Instance.Gold}";
        }
    }
}
