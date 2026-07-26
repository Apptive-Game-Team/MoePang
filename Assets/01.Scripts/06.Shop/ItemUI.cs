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

        private void Awake()
        {
            _upgradePopup = transform.parent.transform.Find("UpgradePopup").gameObject;
            
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
                
                item.transform.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    if (GoldManager.Instance.TrySpendGold(info.price))
                    {
                        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
                        _upgradePopup.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = info.price + "G";
                        _upgradePopup.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "구매하시겠습니까?";
                        _upgradePopup.transform.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
                        _upgradePopup.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
                        {
                            GoldManager.Instance.AdjustGold(-info.price);

                            GameManager.Instance.itemData.ItemAmounts[item.type]++;
                            GameManager.Instance.SaveItemData();
                            
                            item.UpdateAmount();
                        
                            _upgradePopup.SetActive(false);    
                        });
                    
                        _upgradePopup.SetActive(true);
                    }
                });
            }
            
            _upgradePopup.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
                _upgradePopup.SetActive(false);
            });
        }
    }
}
