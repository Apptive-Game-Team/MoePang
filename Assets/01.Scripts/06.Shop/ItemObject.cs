using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;
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
            itemDescription.text = info.itemDescription;
        }

        public void UpdateAmount()
        {
            itemAmount.text = "보유량 " + GameManager.Instance.itemData.ItemAmounts[type];
        }
    }
}
