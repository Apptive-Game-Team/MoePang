using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._06.Shop
{
    public class ItemUI : MonoBehaviour
    {
        public ItemType type;
        [SerializeField] private Image itemImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI itemAmount;
        [SerializeField] private TextMeshProUGUI itemDescription;

        private ShopManager _shopManager;
        private bool _isSelected;

        private void Start()
        {
            Refresh();
        }

        public void Init(ItemInfo info)
        {
            itemImage.sprite = info.sprite;
            itemDescription.text = info.itemDescription;
        }

        public void SetManager(ShopManager manager)
        {
            _shopManager = manager;
        }

        public void OnClick()
        {
            _shopManager.OnClickItem(this);
        }

        public void Select()
        {
            _isSelected = true;
            backgroundImage.color = Color.gray;
        }

        public void Deselect()
        {
            _isSelected = false;
            backgroundImage.color = Color.white;
        }

        public void Refresh()
        {
            int amount = GameManager.Instance.itemData.ItemAmounts[type];

            itemAmount.text = $"{amount} / {999}";
        }
    }
}
