using _01.Scripts._00.Manager;
using _01.Scripts._08.Utility;
using UnityEngine;
using _01.Scripts._11.HabitatMode;

namespace _01.Scripts._04.UI.InGame
{
    public class UIButtons : MonoBehaviour
    {
        public void HomeButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
            SceneFade.Load(SceneInfo.GetSceneName(SceneType.Main));
        }
        
        private void EnterAfterStaminaEffect(System.Action enterStage)
        {
            if (GameManager.Instance.isTestMode)
            {
                if (!StaminaManager.Instance || !StaminaManager.Instance.IsEnteringStage)
                {
                    enterStage();
                }

                return;
            }

            int cost = HabitatModeManager.Instance != null && HabitatModeManager.Instance.IsHabitatBattle
                ? StaminaRules.HabitatEntryCost : StaminaRules.NormalEntryCost;

            if (StaminaManager.Instance)
            {
                StaminaManager.Instance.TryEnterStage(cost, enterStage);
            }
        }

        public void NextStageButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);

            EnterAfterStaminaEffect(LoadNextStage);
        }

        private void LoadNextStage()
        {
            if (!this)
            {
                return;
            }

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
            GameManager.Instance.PlayBattleBGM();
            SceneFade.Load(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
        }

        public void RestartButton()
        {
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);

            EnterAfterStaminaEffect(() =>
            {
                if (!this)
                {
                    return;
                }

                StageManager.Instance.StartStage();
                GameManager.Instance.PlayBattleBGM();
                SceneFade.Load(SceneInfo.GetSceneName(SceneType.MatchAndBattle));
            });
        }
    }
}
