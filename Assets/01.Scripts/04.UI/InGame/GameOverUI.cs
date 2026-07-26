using TMPro;
using UnityEngine;

namespace _01.Scripts._04.UI.InGame
{
    public class GameOverUI : GameUI
    {
        protected override void OnEnable()
        {
            DepthOfField.active = true;
            SoundManager.Instance.PlaySFX(SFX.SFX15_StageFailed);
            
            StageManager.Instance.StopStage();
            Time.timeScale = 0;
        }
    }
}
