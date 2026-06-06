using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._08.Utility
{
    public class SceneMoveButton : MonoBehaviour
    {
        public void TitleMoveButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Title));
        }

        public void MainMoveButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
        }

        public void ShopMoveButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Shop));
        }

        public void BattleMoveButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
        }

        public void HabitatBattleMoveButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.HabitatBattle));
        }

        public void ComboMoveButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Combo));
        }

        public void HabitatModeSelectMoveButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.HabitatModeSelect));
        }

        public void UnitInfoMoveButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.UnitInfo));
        }
    }
}
