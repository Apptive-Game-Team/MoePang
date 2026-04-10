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
            PlayData playData = GameManager.Instance.playData;
            StageManager stageManager = StageManager.Instance;
            GoldManager goldManager = GoldManager.Instance;
            stageManager.StopStage();
            float time = stageManager.CurrentTime;
            int usedTileCount = stageManager.UsedTileCount;
            
            goldManager.AddStageClearedGold();

            playData.goldAmount = goldManager.Gold;
            
            playData.clearedStage = Mathf.Max(playData.clearedStage, stageManager.CurrentStage);
            stageManager.SetMaxStage(playData.clearedStage + 1);

            StageData stageData = playData.stagesData[stageManager.CurrentStage];
            stageData.maxUsedTile = Mathf.Max(stageData.maxUsedTile, usedTileCount);
            stageData.minUsedTile = stageData.minUsedTile == 0 ?
                usedTileCount : Mathf.Min(stageData.maxUsedTile, usedTileCount);
            stageData.minUsedTime = stageData.minUsedTime == 0 ?
                time : Mathf.Min(stageData.minUsedTime, time);
            
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
