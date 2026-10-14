using _01.Scripts._00.Manager;
using UnityEngine;

namespace _01.Scripts._04.UI.MainScene.Ranking
{
    public class RankingCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject rankingList;
        [SerializeField] private GameObject rankingButton;

        private void Awake()
        {
            if (GameManager.Instance.isLoggedIn)
            {
                rankingButton.gameObject.SetActive(true);
            }
        }

        public void OpenRankingList()
        {
            rankingList.SetActive(true);
        }

        public void CloseRankingList()
        {
            rankingList.SetActive(false);
        }
    }
}
