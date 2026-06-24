using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts._08.Utility
{
    public class SceneMoveButton : MonoBehaviour
    {
        public void TitleMoveButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Title));
        }

        public void MainMoveButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
        }

        public void ShopMoveButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Shop));
        }

        public void BattleMoveButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
        }

        public void ComboMoveButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Combo));
        }

        public void HabitatModeSelectMoveButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.HabitatModeSelect));
        }

        public void UnitInfoMoveButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.UnitInfo));
        }
    }
}
