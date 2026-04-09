using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    public class PauseUI : MonoBehaviour
    {
        public void ShowPauseUI(bool show)
        {
            Time.timeScale = show ? 0 : 1;
            gameObject.SetActive(show);
        }
    }
}
