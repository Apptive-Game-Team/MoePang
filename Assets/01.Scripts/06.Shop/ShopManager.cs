using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("씬 연결")]
    [SerializeField] private string prevScene;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    [Header("패널")]
    [SerializeField] private List<GameObject> panels;
    [SerializeField] private List<GameObject> upgradePanels;
    [SerializeField] private GameObject animalsTap; //동물 해금 탭
    [SerializeField] private GameObject upgradeTap; //강화 탭

    [Header("버튼")]
    [SerializeField] private Button animalsTapButton;
    [SerializeField] private Button upgradeTapButton;
    
    [Header("상태")]
    [SerializeField] private bool isUnitClicked = false;
    [SerializeField] private ShopUI currentSelected;
    [SerializeField] private UpgradeUI currentUpgradeSelected;

    private List<ShopUI> allShopUI = new List<ShopUI>();

    private void Awake()
    {
        EnsureActivateAnimalsPanel();
        foreach (var panel in panels)
        {
            ShopUI[] ui = panel.GetComponentsInChildren<ShopUI>(true);

            foreach (var temp in ui)
            {
                temp.SetManager(this);
            }

            UpgradeUI[] upgradeUI = panel.GetComponentsInChildren<UpgradeUI>(true);
            foreach (var temp in upgradeUI)
            {
                temp.SetManager(this);
            }
        }
    }

    private void Start()
    {
        goldText.text = $"Gold : {GoldManager.Instance.Gold}";
        animalsTap.SetActive(true);
        upgradeTap.SetActive(false);
        UpdateBuyButtonText();
    }
    public void OnClickBack()
    {
        SceneManager.LoadScene(prevScene);
    }

    public void OnClickUnit(ShopUI clickedUI)
    {
        if (currentSelected == clickedUI)
        {
            currentSelected.Deselect();
            currentSelected = null;
            UpdateBuyButtonText();
            return;
        }

        if (currentSelected != null)
        {
            currentSelected.Deselect();
        }

        currentSelected = clickedUI;
        currentSelected.Select();

        UpdateBuyButtonText();
    }

    public void OnClickUpgrade(UpgradeUI clickedUI)
    {
        if (currentUpgradeSelected == clickedUI)
        {
            currentUpgradeSelected.Deselect();
            currentUpgradeSelected = null;
            UpdateBuyButtonText();
            return;
        }

        if (currentUpgradeSelected != null)
        {
            currentUpgradeSelected.Deselect();
        }

        currentUpgradeSelected = clickedUI;
        currentUpgradeSelected.Select();

        UpdateBuyButtonText();
    }

    private void UpdateBuyButtonText()
    {
        if (currentUpgradeSelected != null)
        {
            var data = currentUpgradeSelected.Data;

            if (!UpgradeManager.Instance.CanUpgrade(data))
            {
                buyButtonText.text = "MAX";
                return;
            }

            int cost = UpgradeManager.Instance.GetCost(data);
            buyButtonText.text = $"비용 : {cost}G";
            return;
        }

        if (currentSelected == null)
        {
            buyButtonText.text = "골라주세용";
            return;
        }

        var unit = currentSelected.UnitData;

        if (currentSelected.IsUnlocked)
        {
            buyButtonText.text = "OwO";
            return;
        }

        if (!HabitatManager.Instance.CanUnlock(unit))
        {
            buyButtonText.text = "잠겨있음";
            return;
        }

        buyButtonText.text = $"비용 : {unit.UnitCost}G";
    }

    public void OnClickBuy()
    {
        if (currentUpgradeSelected != null)
        {
            var data = currentUpgradeSelected.Data;

            if (!UpgradeManager.Instance.CanUpgrade(data))
            {
                buyButtonText.text = "MAX";
                return;
            }

            int cost = UpgradeManager.Instance.GetCost(data);

            if (!GoldManager.Instance.TrySpendGold(cost))
            {
                buyButtonText.text = "돈없엉";
                return;
            }

            UpgradeManager.Instance.Upgrade(data);

            goldText.text = $"Gold : {GoldManager.Instance.Gold}";
            currentUpgradeSelected.Refresh();
            UpdateBuyButtonText();
            return;
        }

        if (currentSelected == null)
            return;

        var unit = currentSelected.UnitData;

        if (currentSelected.IsUnlocked)
        {
            buyButtonText.text = "OwO";
            return;
        }

        if (!HabitatManager.Instance.CanUnlock(unit))
        {
            buyButtonText.text = "앞에꺼사.";
            return;
        }

        if (!GoldManager.Instance.TrySpendGold(unit.UnitCost))
        {
            buyButtonText.text = "돈없엉";
            return;
        }

        HabitatManager.Instance.Unlock(unit);

        currentSelected.RefreshUnlockState();

        goldText.text = $"Gold : {GoldManager.Instance.Gold}";
        buyButtonText.text = "굿굿";
    }

    private void EnsureActivateAnimalsPanel()
    {
        ActivateAnimalsTap();
    }

    #region 탭 전환 버튼

    /// <summary>
    /// 해금탭을 비활성화 하고 강화탭을 활성화
    /// </summary>
    public void ActivateUpgradeTap()
    {
        if (upgradeTap.activeSelf) return;
        
        animalsTap.SetActive(false);
        upgradeTap.SetActive(true);
    }

    /// <summary>
    /// 강화탭을 비활성화 하고 해금탭을 활성화
    /// </summary>
    public void ActivateAnimalsTap()
    {
        if (animalsTap.activeSelf) return;
        
        upgradeTap.SetActive(false);
        animalsTap.SetActive(true);
    }

    #endregion
}
