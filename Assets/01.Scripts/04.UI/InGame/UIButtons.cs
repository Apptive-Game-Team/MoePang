using _01.Scripts._08.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._04.UI.InGame
{
    public class UIButtons : MonoBehaviour
    {
        public void HomeButton()
        {
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
        }
        
        public void NextStageButton()
        {
            StageManager.Instance.AddStage(1);
            StageManager.Instance.StartStage();
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
        }

        public void RestartButton()
        {
            StageManager.Instance.StartStage();
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
        }
    }
}
