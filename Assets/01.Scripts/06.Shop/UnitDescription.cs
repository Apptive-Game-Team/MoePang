using _01.Scripts._00.Manager;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace _01.Scripts._06.Shop
{
    /// <summary>
    /// Unit Description Scene의 유닛 설명 표시 스크립트
    /// </summary>
    public class UnitDescription : MonoBehaviour
    {
        [Header("Description Setting")]
        [SerializeField] private Image habitatImage;
        [SerializeField] private List<Sprite> habitatSprites = new List<Sprite>();
        [SerializeField] private TextMeshProUGUI habitatText;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI unitLevelText;
        [SerializeField] private TextMeshProUGUI unitAttackTypeText;
        [SerializeField] private TextMeshProUGUI unitStatText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Animator unitAnimator;

        private void Start()
        {
            RefreshDescription();
        }

        public void RefreshDescription()
        {
            FriendlyUnitData data = HabitatManager.Instance.SelectedUnitData;

            if (data == null)
            {
                return;
            }
            ApplyHabitatImage(data.Habitat);

            habitatText.text = GetHabitatText(data.Habitat);
            unitNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", data.UnitName.ToString());
            unitLevelText.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Lv")} {data.UnitLevel}";
            unitAttackTypeText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", data.AttackType.ToString());
            unitStatText.text = GetUnitStatText(data);
            descriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", data.UnitDescriptionText);
            unitAnimator.runtimeAnimatorController = data.AnimatorOverride;
            unitAnimator.Play("Walk", 0, 0f);
            
            bool unlocked = HabitatManager.Instance.IsUnlocked(data);
            int cost = unlocked ? data.UnitCost : data.UnlockCost;
        }

        private string GetUnitStatText(FriendlyUnitData data)
        {
            int stage = GetStatPreviewStage();
            float attackDamage = BalanceFormula.GetUnitAttackDamage(data.AttackDamage, data.UnitLevel, data.UnitGrade, stage);
            float maxHp = BalanceFormula.GetUnitMaxHp(data.MaxHp, data.UnitLevel, data.UnitGrade, stage);

            return $"{attackDamage}\n{maxHp}";
        }

        private int GetStatPreviewStage()
        {
            StageManager stageManager = FindObjectOfType<StageManager>();
            if (stageManager != null)
            {
                return Mathf.Max(1, stageManager.MaxStage);
            }

            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null &&
                gameManager.playData != null &&
                gameManager.playData.MaxStages.TryGetValue(StageType.Normal, out int maxStage))
            {
                return Mathf.Max(1, maxStage);
            }

            return 1;
        }
    
        /// <summary>
        /// 좌측 상단 서식지 이미지 변경
        /// </summary>
        private void ApplyHabitatImage(Habitat habitat)
        {
            int index = habitat switch
            {
                Habitat.Meadow => 0,
                Habitat.Ocean => 1,
                Habitat.Desert => 2,
                Habitat.Forest => 3,
                Habitat.Polar => 4,
                _ => -1
            };

            if (index >= 0 && index < habitatSprites.Count)
            {
                habitatImage.sprite = habitatSprites[index];
                habitatImage.gameObject.SetActive(true);
            }
            else
            {
                habitatImage.gameObject.SetActive(false);
            }
        }

        private string GetHabitatText(Habitat habitat)
        {
            string key = habitat switch
            {
                Habitat.Meadow => "Habitat_Meadow",
                Habitat.Ocean => "Habitat_Ocean",
                Habitat.Desert => "Habitat_Desert",
                Habitat.Forest => "Habitat_Forest",
                Habitat.Polar => "Habitat_Polar",
                _ => habitat.ToString()
            };

            return LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", key);
        }
    }
}
