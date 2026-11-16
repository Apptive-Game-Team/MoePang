using _01.Scripts._00.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace _01.Scripts._06.Shop
{
    public class CastleUpgradeUI : MonoBehaviour
    {
        [Header("Castle Upgrade Settings")]
        [SerializeField] private TextMeshProUGUI castleLevel;
        [SerializeField] private TextMeshProUGUI castleDescription;
        [SerializeField] private Button castleUpgradeButton;

        private GameObject _upgradePopup;
        private int _castleLevel;

        private void Awake()
        {
            _castleLevel = GameManager.Instance.castleData.castleLevel;
            _upgradePopup = transform.parent.transform.Find("CastleUpgradePopup").gameObject;

            UpdateText();
            RegisterUpgradeButton();
        }

        private void UpdateText()
        {
            string currentHpText = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "07CastleText1");
            string upgradedHpText = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "07CastleText2");

            castleLevel.text = $"{LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Lv")}{_castleLevel}";
            castleDescription.text = $"{currentHpText} : {_castleLevel * BalanceFormula.CastleHpIncreasePerLevel}\n" +
                                     $"{upgradedHpText} : {(_castleLevel + 1) * BalanceFormula.CastleHpIncreasePerLevel}";
        }

        private void RegisterUpgradeButton()
        {
            if (_castleLevel >= 21)
            {
                castleUpgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Level";
                castleUpgradeButton.interactable = false;
            }

            castleUpgradeButton.onClick.AddListener(() =>
            {
                int cost = GetUpgradeCost();

                if (GoldManager.Instance.TrySpendGold(cost))
                {
                    _upgradePopup.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text =
                        LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "07CastleUpgrade");
                    _upgradePopup.transform.Find("CostText").GetComponent<TextMeshProUGUI>().text = GetCostText(cost);
                    _upgradePopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.RemoveAllListeners();
                    _upgradePopup.transform.Find("AcceptButton").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GoldManager.Instance.AdjustGold(-cost);
                        SoundManager.Instance.PlaySFX(SFX.SFX12_PurChase);

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

                    SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
                    _upgradePopup.SetActive(true);
                }
            });

            _upgradePopup.transform.Find("CloseButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
                _upgradePopup.SetActive(false);
            });
        }

        private int GetUpgradeCost()
        {
            return _castleLevel <= 5
                ? _castleLevel * BalanceFormula.CastleHpBaseCost
                : 600 + BalanceFormula.CastleHpBaseCost * (_castleLevel - 6);
        }

        private string GetCostText(int cost)
        {
            string levelText = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "Lv");
            string usedGoldText = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "UsedGold");
            string goldUnitText = LocalizationSettings.StringDatabase.GetLocalizedString("LocalizationDataTable", "07G");

            return $"{levelText}{_castleLevel} => {levelText}{_castleLevel + 1}\n" +
                   $"{usedGoldText} {cost}{goldUnitText}";
        }
    }
}
