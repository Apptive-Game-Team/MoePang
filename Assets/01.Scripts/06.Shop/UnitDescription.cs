using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace _01.Scripts._06.Shop
{
    public class UnitDescription : MonoBehaviour
    {
        [Header("Description Setting")]
        [SerializeField] private Image habitatImage;
        [SerializeField] private List<Sprite> habitatSprites = new List<Sprite>();
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Animator unitAnimator;
        [SerializeField] private TextMeshProUGUI unitStatText;
        [SerializeField] private TextMeshProUGUI unitUpgradeCostText;

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

            unitNameText.text = data.UnitName.ToString();
            descriptionText.text = data.UnitDescriptionText;
            unitAnimator.runtimeAnimatorController = data.AnimatorOverride;
            unitAnimator.Play("Walk", 0, 0f);

            unitStatText.text =
                $"Level : {data.UnitLevel}\n" +
                $"Current Grade : {data.UnitGrade}\n" +
                $"Attack Type : {data.AttackType}\n" +
                $"HP : {data.MaxHp}\n" +
                $"Damage : {data.AttackDamage}\n" +
                $"Attack Speed : {data.AttackSpeed}\n" +
                $"Move Speed : {data.BaseMoveSpeed}";
            bool unlocked = HabitatManager.Instance.IsUnlocked(data);
            int cost = unlocked ? data.UnitCost : data.UnlockCost;
            unitUpgradeCostText.text = unlocked ? $"Level Up : {cost}" : $"Unlock : {cost}";
        }
    
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
    }
}
