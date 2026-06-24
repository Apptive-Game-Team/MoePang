using _01.Scripts._08.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._04.UI.InGame
{
    public class UIButtons : MonoBehaviour
    {
        public void HomeButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
        }
        
        public void NextStageButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            StageManager.Instance.AddStage(1);
            StageManager.Instance.StartStage();
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
        }

        public void RestartButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            StageManager.Instance.StartStage();
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
        }
    }
}
