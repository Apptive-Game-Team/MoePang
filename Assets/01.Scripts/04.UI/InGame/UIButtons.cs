using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._04.UI.InGame
{
    public class UIButtons : MonoBehaviour
    {
        public void HomeButton()
        {
            SceneManager.LoadScene(1);
        }
        
        public void NextStageButton()
        {
            StageManager.Instance.AddStage(1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void RestartButton()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
