using _01.Scripts._00.Manager;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._06.Shop
{
    public class ItemUI : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;
        private GameObject _upgradePopup;
        private int maxBuyCount = 99;
        private int _buyCount = 1;
        private ItemInfo _selectedInfo;
        private ItemObject _selectedItem;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _unitPriceText;
        private TextMeshProUGUI _countText;
        private TextMeshProUGUI _totalPriceText;
        private Button _plusButton;
        private Button _minusButton;
        private Button _confirmButton;
        private Button _cancelButton;
        

        private void Awake()
        {
            _upgradePopup = transform.parent.transform.Find("UpgradePopup").gameObject;
            
            _titleText = _upgradePopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _unitPriceText = _upgradePopup.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            _countText = _upgradePopup.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            _totalPriceText = _upgradePopup.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
          
            _plusButton = _upgradePopup.transform.GetChild(4).GetComponent<Button>();
            _minusButton = _upgradePopup.transform.GetChild(5).GetComponent<Button>();
            _confirmButton = _upgradePopup.transform.GetChild(6).GetComponent<Button>();
            _cancelButton = _upgradePopup.transform.GetChild(7).GetComponent<Button>();
            
            _minusButton.onClick.AddListener(() => ChangeBuyCount(-1));
            _plusButton.onClick.AddListener(() => ChangeBuyCount(1));
            _cancelButton.onClick.AddListener(ClosePopup);
            _confirmButton.onClick.AddListener(BuySelectedItem);
            InitialSetting();
        }

        private void InitialSetting()
        {
            List<ItemObject> items = new(transform.GetComponentsInChildren<ItemObject>());

            foreach (ItemObject item in items)
            {
                ItemInfo info = itemData.items.Find(i => i.type == item.type);
                
                item.Init(info);
                item.UpdateAmount();
                
                ItemObject currentItem = item;
                ItemInfo currentInfo = info;

                item.transform.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    OpenBuyPopup(currentItem, currentInfo);
                });
            }
        }
        
        private void OpenBuyPopup(ItemObject item, ItemInfo info)
            {
                if (!GoldManager.Instance.TrySpendGold(info.price))
                    return;

                SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);

                _selectedItem = item;
                _selectedInfo = info;
                _buyCount = 1;

                RefreshPopupText();
                _upgradePopup.SetActive(true);
            }
            
            private void ChangeBuyCount(int amount)
            {
                if (_selectedInfo == null)
                    return;

                SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);

                int affordableCount = GoldManager.Instance.Gold / _selectedInfo.price;
                int limit = Mathf.Min(maxBuyCount, affordableCount);

                _buyCount = Mathf.Clamp(_buyCount + amount, 1, limit);

                RefreshPopupText();
            }

            private void RefreshPopupText()
            {
                int totalPrice = _selectedInfo.price * _buyCount;

                _titleText.text = _selectedInfo.type + "을 구매합니다.";
                _unitPriceText.text = _selectedInfo.price + "G";
                _countText.text = _buyCount.ToString();
                _totalPriceText.text = totalPrice + "G";
            }

            private void BuySelectedItem()
            {
                if (_selectedInfo == null || _selectedItem == null)
                    return;

                int totalPrice = _selectedInfo.price * _buyCount;

                if (!GoldManager.Instance.TrySpendGold(totalPrice))
                    return;

                GoldManager.Instance.AdjustGold(-totalPrice);
                SoundManager.Instance.PlaySFX(SFX.SFX12_PurChase);

                GameManager.Instance.itemData.ItemAmounts[_selectedItem.type] += _buyCount;
                GameManager.Instance.SaveItemData();

                _selectedItem.UpdateAmount();

                ClosePopup();
            }

            private void ClosePopup()
            {
                SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
                _upgradePopup.SetActive(false);
            }
    }
}
