using UnityEngine;

namespace _01.Scripts._04.UI.MainScene.Ranking
{
    public class RankingCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject rankingList;

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
