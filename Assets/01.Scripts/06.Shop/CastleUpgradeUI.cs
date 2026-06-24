using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._06.Shop
{
    public class CastleUpgradeUI : MonoBehaviour
    {
        [SerializeField] private UpgradeData castleData;
        [SerializeField] private TextMeshProUGUI castleLevel;
        [SerializeField] private TextMeshProUGUI castleDescription;
        [SerializeField] private Button castleUpgradeButton;
        private GameObject _upgradePopup;

        private int _castleLevel;

        private void Awake()
        {
            _castleLevel = GameManager.Instance.castleData.castleLevel;
            _upgradePopup = transform.parent.transform.Find("UpgradePopup").gameObject;
            
            UpdateText();
            RegisterUpgradeButton();
        }

        private void UpdateText()
        {
            castleLevel.text = $"LV{_castleLevel}";
            castleDescription.text = $"현재 서식지의 체력 : {_castleLevel * castleData.IncreasePerLevel}\n" +
                                     $"강화 시 : {(_castleLevel + 1) * castleData.IncreasePerLevel}";
        }

        private void RegisterUpgradeButton()
        {
            if (_castleLevel >= 21)
            {
                castleUpgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Level";
                castleUpgradeButton.interactable = false;
            }
            
            int cost = _castleLevel <= 5
                ? _castleLevel * castleData.BaseCost
                : 600 + castleData.BaseCost * (_castleLevel - 6);
            
            castleUpgradeButton.onClick.AddListener(() =>
            {
                if (GoldManager.Instance.TrySpendGold(cost))
                {
                    _upgradePopup.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = cost + "G";
                    _upgradePopup.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "강화하시겠습니까?";
                    _upgradePopup.transform.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
                    _upgradePopup.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GoldManager.Instance.AdjustGold(-cost);
                        
                        _castleLevel++;
                        GameManager.Instance.castleData.castleLevel = _castleLevel;
                        GameManager.Instance.SaveCastleData();

                        UpdateText();
                        
                        if (_castleLevel >= 21)
                        {
                            castleUpgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Level";
                            castleUpgradeButton.interactable = false;
                        }
                        
                        _upgradePopup.SetActive(false);    
                    });
                    
                    _upgradePopup.SetActive(true);
                }
            });

            _upgradePopup.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                _upgradePopup.SetActive(false);
            });
        }
    }
}
