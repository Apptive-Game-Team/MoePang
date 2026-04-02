using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts._04.UI.InGame
{
    public class GameClearUI : GameUI
    {
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI coinText;

        protected override void OnEnable()
        {
            Time.timeScale = 0.1f;
            DepthOfField.active = true;
            stageText.text = $"지켜낸 서식지 {StageManager.Instance.CurrentStage}";
            // todo : 시간 설정
            coinText.text = $"{GoldManager.Instance.Gold}";
        }
    }
}
