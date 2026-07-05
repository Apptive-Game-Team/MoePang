using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    public class PauseUI : MonoBehaviour
    {
        public void ShowPauseUI(bool show)
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            Time.timeScale = show ? 0 : 1;
            gameObject.SetActive(show);
        }

        private void OnDisable()
        {
            Time.timeScale = 1;
        }
    }
}
