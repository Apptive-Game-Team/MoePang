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
            Time.timeScale = 0.1f;
            // todo : 시간 설정
            coinText.text = $"{GoldManager.Instance.Gold}";
        }
    }
}
