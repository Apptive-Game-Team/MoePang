using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using _01.Scripts._11.HabitatMode;
using UnityEngine.UI;

namespace _01.Scripts._04.UI.InGame
{
    public class GameClearUI : GameUI
    {
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private GameObject diaInfo;
        [SerializeField] private TextMeshProUGUI diaText;
        [SerializeField] private Image comboUnlockedPopup;
        [SerializeField] private Sprite[] habitatSprites;
        [Header("Clear Reward Animation")]
        [SerializeField, Min(0)] private float goldCountDelay = 0.5f;
        [SerializeField, Min(0.01f)] private float goldCountDuration = 0.9f;

        private Vector2? _timeRowPosition;
        private Vector2 _goldRowPosition;

        protected override void OnEnable()
        {
            DepthOfField.active = true;
            SoundManager.Instance.PlaySFX(SFX.SFX14_StageClear);

            GameManager gameManager = GameManager.Instance;
            StageManager stageManager = StageManager.Instance;
            GoldManager goldManager = GoldManager.Instance;
            HabitatModeManager habitatModeManager = HabitatModeManager.Instance;
            float time = stageManager.CurrentTime;

            stageManager.StopStage();
            int goldBeforeReward = goldManager.Gold;
            goldManager.AddStageClearedGold();
            int goldAfterReward = goldManager.Gold;

            int diaAmount = 0;
            bool isHabitatBattle =
                habitatModeManager != null &&
                habitatModeManager.IsHabitatBattle;

            if (isHabitatBattle)
            {
                diaAmount = goldManager.AddStageClearedDia();
                SetComboUnlockedPopup();
            }

            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            Time.timeScale = 0f;
            DepthOfField.active = true;
            string protectedText = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "GameClearUI_Protected");
            string habitatText = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "GameClearUI_Habitat");

            if (isHabitatBattle)
            {
                HabitatMode mode = habitatModeManager.HabitatMode;
                string habitatName = GetHabitatName(mode);
                int habitatStage = stageManager.GetHabitatStage(mode) + 1;

                stageText.text = $"{protectedText} {habitatName} {habitatText} {habitatStage}";
            }
            else
            {
                stageText.text = $"{protectedText} {habitatText} {stageManager.CurrentStage + 1}";
            }
            timeText.text = $"{minutes}:{seconds:00}";
            SetRewardLayout(isHabitatBattle);
            PlayAmountEffect(coinText, goldBeforeReward, goldAfterReward);

            if (isHabitatBattle && diaText != null)
            {
                PlayAmountEffect(diaText, 0, diaAmount);
            }

            gameManager.SavePlayData();
        }

        private void SetRewardLayout(bool showDia)
        {
            if (diaInfo == null)
            {
                return;
            }

            var timeRow = (RectTransform)timeText.transform.parent;
            var goldRow = (RectTransform)coinText.transform.parent;
            var diaRow = (RectTransform)diaInfo.transform;

            if (!_timeRowPosition.HasValue)
            {
                _timeRowPosition = timeRow.anchoredPosition;
                _goldRowPosition = goldRow.anchoredPosition;
            }

            Vector2 timePosition = _timeRowPosition.Value;
            float offsetY = showDia ? 0f : (diaRow.anchoredPosition.y - _goldRowPosition.y) * 0.5f;
            Vector2 offset = new Vector2(0f, offsetY);
            timeRow.anchoredPosition = timePosition + offset;
            goldRow.anchoredPosition = _goldRowPosition + offset;
            diaInfo.SetActive(showDia);
        }

        private void PlayAmountEffect(TextMeshProUGUI amountText, int from, int to)
        {
            var effect = amountText.GetComponent<CurrencyAmountEffect>();

            if (effect == null)
            {
                effect = amountText.gameObject.AddComponent<CurrencyAmountEffect>();
            }

            effect.Play(amountText, from, to, goldCountDelay, goldCountDuration);
        }

        private string GetHabitatName(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => "초원",
                HabitatMode.OceanMode => "바다",
                HabitatMode.DesertMode => "사막",
                HabitatMode.ForestMode => "숲",
                HabitatMode.PolarMode => "극지",
                _ => mode.ToString()
            };
        }

        private void SetComboUnlockedPopup()
        {
            HabitatMode mode = HabitatModeManager.Instance.HabitatMode;
            StageType type = GameManager.Instance.GetStageTypeWithHabitat(mode);
            Image habitatImage = comboUnlockedPopup.transform.GetChild(1).GetComponent<Image>();

            if (GameManager.Instance.playData.MaxStages[type] == 4 &&
                StageManager.Instance.GetHabitatStage(mode) == 4)
            {
                habitatImage.sprite = habitatSprites[(int)mode];

                Button button = comboUnlockedPopup.GetComponentInChildren<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    comboUnlockedPopup.gameObject.SetActive(false);
                });

                comboUnlockedPopup.gameObject.SetActive(true);
            }
        }
    }
}
