using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Ranking
{
    public class RankerProfileUI : MonoBehaviour
    {
        [SerializeField] private Image rankerImage;
        [SerializeField] private TextMeshProUGUI rankerNickname;
        [SerializeField] private TextMeshProUGUI rankerMaxStage;
        [SerializeField] private TextMeshProUGUI rankerMinUsedTime;
        [SerializeField] private TextMeshProUGUI[] rankerHabitatBonuses;

        public void InitialSetting(RankingEntry entry, Sprite sprite)
        {
            rankerImage.sprite = sprite;
            rankerNickname.text = entry.Nickname;
            rankerMaxStage.text = $"stage {entry.MaxStage}";
            rankerMinUsedTime.text = $"{entry.MinUsedTime:F}".Replace(".", " : ");
            for (int k = 0; k < rankerHabitatBonuses.Length; k++)
            {
                rankerHabitatBonuses[k].text = "+" + entry.StageProgress[k + 1];
            }
        }

        public void CloseRankerUI()
        {
            gameObject.SetActive(false);
        }
    }
}
