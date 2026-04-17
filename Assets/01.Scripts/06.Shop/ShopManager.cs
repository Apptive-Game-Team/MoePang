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

    [Header("구매 팝업")]
    [SerializeField] private GameObject buyPopup;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI tooltipText;

    //상태 제어
    private ShopUI currentSelected;
    private UpgradeUI currentUpgradeSelected;
    private bool isBuyPopupActive = false;

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
        buyPopup.SetActive(false);
        UpdateBuyButtonText();
    }
    public void OnClickBack()
    {
        SceneManager.LoadScene(prevScene);
    }

    public void OnClickUnit(ShopUI clickedUI)
    {
        if (isBuyPopupActive) return;

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
            buyButtonText.text = $"Buy";
            return;
        }

        if (currentSelected == null)
        {
            buyButtonText.text = "Buy";
            return;
        }

        var unit = currentSelected.UnitData;

        if (currentSelected.IsUnlocked)
        {
            buyButtonText.text = "구매완료";
            return;
        }

        if (!HabitatManager.Instance.CanUnlock(unit))
        {
            buyButtonText.text = "잠겨있음";
            return;
        }

        buyButtonText.text = $"Buy";
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

            buyPopup.SetActive(true);
            isBuyPopupActive = true;
            costText.text = $"Cost : {cost}G";
            tooltipText.text = $"{data.UpgradeType}을 강화하겠습니까? " +
                $"\n {data.UpgradeType} += {data.IncreasePerLevel}";
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

        buyPopup.SetActive(true);
        isBuyPopupActive = true;
        costText.text = $"Cost : {unit.UnitCost}G";
        tooltipText.text = $"{unit.UnitName}을 해금하겠습니까?";
    }

    /// <summary>
    /// 팝업의 [확인] 버튼에 연결할 실제 구매 로직
    /// </summary>
    public void ConfirmPurchase()
    {
        if (currentUpgradeSelected != null)
        {
            var data = currentUpgradeSelected.Data;
            int cost = UpgradeManager.Instance.GetCost(data);

            if (GoldManager.Instance.TrySpendGold(cost))
            {
                UpgradeManager.Instance.Upgrade(data);
                currentUpgradeSelected.Refresh();
            }
        }

        else if (currentSelected != null)
        {
            var unit = currentSelected.UnitData;

            if (GoldManager.Instance.TrySpendGold(unit.UnitCost))
            {
                HabitatManager.Instance.Unlock(unit);
                currentSelected.RefreshUnlockState();
            }
        }

        goldText.text = $"Gold : {GoldManager.Instance.Gold}";
        UpdateBuyButtonText();
        ClosePopup();
    }

    /// <summary>
    /// 팝업의 [취소] 버튼이나 닫기 버튼에 연결
    /// </summary>
    public void ClosePopup()
    {
        if (buyPopup != null)
        {
            isBuyPopupActive = false;
            buyPopup.SetActive(false);
        }
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
        if (upgradeTap.activeSelf || isBuyPopupActive) return;
        
        animalsTap.SetActive(false);
        upgradeTap.SetActive(true);
    }

    /// <summary>
    /// 강화탭을 비활성화 하고 해금탭을 활성화
    /// </summary>
    public void ActivateAnimalsTap()
    {
        if (animalsTap.activeSelf || isBuyPopupActive) return;
        
        upgradeTap.SetActive(false);
        animalsTap.SetActive(true);
    }

    #endregion
}
