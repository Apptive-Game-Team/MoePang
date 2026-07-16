using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using _01.Scripts._11.HabitatMode;

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
            if (HabitatModeManager.Instance != null && HabitatModeManager.Instance.IsHabitatBattle)
            {
                StageType stageType =
                    GameManager.Instance.GetStageTypeWithHabitat(HabitatModeManager.Instance.HabitatMode);
                
                if (GameManager.Instance.playData.selectedStage[(int)stageType] <
                    GameManager.Instance.playData.MaxStages[stageType])
                {
                    StageManager.Instance.AddHabitatStage(HabitatModeManager.Instance.HabitatMode, 1);
                }
            }
            else
            {
                StageType stageType = StageType.Normal;
                
                if (GameManager.Instance.playData.selectedStage[(int)stageType] <
                    GameManager.Instance.playData.MaxStages[stageType])
                {
                    StageManager.Instance.AddStage(1);
                }
            }
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
