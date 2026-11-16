using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.MainScene.Ranking
{
    public class RankingUserData : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rankingText;
        [SerializeField] private Image profileImage;
        [SerializeField] private TextMeshProUGUI profileNickname;
        [SerializeField] private TextMeshProUGUI maxStage;

        public void ProfileSetting(int rank, Sprite image, string nickname, int stage, float minTime)
        {
            rankingText.text = rank.ToString();
            profileNickname.text = nickname;
            maxStage.text = "Stage : " + stage + "\n" + minTime.ToString("F");

            if (image != null)
            {
                profileImage.sprite = image;
                profileImage.enabled = true;
            }
        }

        public void SetProfileImage(Sprite image)
        {
            if (!image)
            {
                return;
            }

            profileImage.sprite = image;
            profileImage.enabled = true;
        }
    }
}