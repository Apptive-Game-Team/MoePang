using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace _01.Scripts._06.Shop
{
    public class ItemObject : MonoBehaviour
    {
        public ItemType type;
        [SerializeField] private Image itemImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI itemAmount;
        [SerializeField] private TextMeshProUGUI itemDescription;

        public void Init(ItemInfo info)
        {
            itemImage.sprite = info.sprite;
            itemDescription.text = $"{GetItemName(info.type)}\n{GetItemDescription(info.type)}";
        }

        public void UpdateAmount()
        {
            string holdingsText = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "07Holdings");
            itemAmount.text = $"{holdingsText} : {GameManager.Instance.itemData.ItemAmounts[type]}";
        }

        private string GetItemName(ItemType itemType)
        {
            string key = itemType switch
            {
                ItemType.Joker => "07Joker",
                ItemType.DestroyObstacle => "07Pure",
                ItemType.CreateLineBomb => "07Support",
                ItemType.RaiseSpawnProb => "07Potion",
                _ => itemType.ToString()
            };

            return LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", key);
        }

        private string GetItemDescription(ItemType itemType)
        {
            string key = itemType switch
            {
                ItemType.Joker => "07JokerDescription",
                ItemType.DestroyObstacle => "07PureDescription",
                ItemType.CreateLineBomb => "07SupportDescription",
                ItemType.RaiseSpawnProb => "07PotionDescription",
                _ => itemType.ToString()
            };

            return LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", key);
        }
    }
}
