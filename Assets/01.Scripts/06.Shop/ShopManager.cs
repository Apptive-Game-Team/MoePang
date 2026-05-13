using _01.Scripts._00.Manager;
using _01.Scripts._06.Shop;
using _01.Scripts._08.Utility;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ItemData = _01.Scripts._06.Shop.ItemData;

public class ShopManager : MonoBehaviour
{
    [Header("데이터")] 
    [SerializeField] private ItemData itemData;
    
    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    [Header("패널")]
    [SerializeField] private List<GameObject> panels;
    [SerializeField] private List<GameObject> upgradePanels;
    [SerializeField] private GameObject animalsTap; //동물 해금 탭
    [SerializeField] private GameObject upgradeTap; //강화 탭
    [SerializeField] private GameObject itemTap; // 소모품 탭
    [SerializeField] private Image[] buttonImages;
    [SerializeField] private Sprite[] buttonSprite;

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
    private ItemUI currentItemSelected;
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
            
            ItemUI[] itemUI = panel.GetComponentsInChildren<ItemUI>(true);
            foreach (var temp in itemUI)
            {
                temp.SetManager(this);
                temp.Init(itemData.items.First(info => info.type == temp.type));
            }
        }
    }

    private void Start()
    {
        goldText.text = $"{GoldManager.Instance.Gold}";
        animalsTap.SetActive(true);
        upgradeTap.SetActive(false);
        buyPopup.SetActive(false);
        UpdateBuyButtonText();
    }

    private void OnEnable()
    {
        GoldManager.Instance.OnGoldChanged += OnGoldChanged;
    }

    private void OnDisable()
    {
        GoldManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged()
    {
        goldText.text = $"{GoldManager.Instance.Gold}";
    }
    
    public void OnClickBack()
    {
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
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

    public void OnClickItem(ItemUI clickedUI)
    {
        if (currentItemSelected == clickedUI)
        {
            currentItemSelected.Deselect();
            currentItemSelected = null;
            UpdateBuyButtonText();
            return;
        }

        if (currentItemSelected != null)
        {
            currentItemSelected.Deselect();
        }

        currentItemSelected = clickedUI;
        currentItemSelected.Select();
        
        UpdateBuyButtonText();
    }

    private void UpdateBuyButtonText()
    {
        if (currentItemSelected != null)
        {
            if (GameManager.Instance.itemData.ItemAmounts[currentItemSelected.type] >= 999)
            {
                buyButtonText.text = "MAX";
                return;
            }
            
            buyButtonText.text = "Buy";
            return;
        }
        
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
        if (currentItemSelected != null)
        {
            if (GameManager.Instance.itemData.ItemAmounts[currentItemSelected.type] >= 999)
            {
                buyButtonText.text = "MAX";
                return;
            }

            int cost = itemData.items.First(info => info.type == currentItemSelected.type).price;
            
            if (!GoldManager.Instance.TrySpendGold(cost))
            {
                buyButtonText.text = "돈없엉";
                return;
            }
            
            buyPopup.SetActive(true);
            isBuyPopupActive = true;
            costText.text = $"Cost : {cost}G";
            tooltipText.text = $"{currentItemSelected.type}을 구매하시겠습니까?";
            return;
        }
        
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
        {
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
                GoldManager.Instance.AdjustGold(-cost);
                UpgradeManager.Instance.Upgrade(data);
                currentUpgradeSelected.Refresh();
            }
        }

        else if (currentSelected != null)
        {
            var unit = currentSelected.UnitData;

            if (GoldManager.Instance.TrySpendGold(unit.UnitCost))
            {
                GoldManager.Instance.AdjustGold(-unit.UnitCost);
                HabitatManager.Instance.Unlock(unit);
                currentSelected.RefreshUnlockState();
            }
        }
        
        else if (currentItemSelected != null)
        {
            int cost = itemData.items.First(info => info.type == currentItemSelected.type).price;

            if (GoldManager.Instance.TrySpendGold(cost) &&
                GameManager.Instance.itemData.ItemAmounts[currentItemSelected.type] < 999)
            {
                GoldManager.Instance.AdjustGold(-cost);
                GameManager.Instance.itemData.ItemAmounts[currentItemSelected.type]++;
                currentItemSelected.Refresh();
            }
        }
        
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
        if (upgradeTap.activeSelf || isBuyPopupActive)
        {
            return;
        }
        
        currentItemSelected = null;
        currentSelected = null;
        
        animalsTap.SetActive(false);
        buttonImages[0].sprite = buttonSprite[0];
        itemTap.SetActive(false);
        buttonImages[2].sprite = buttonSprite[0];
        
        upgradeTap.SetActive(true);
        buttonImages[1].sprite = buttonSprite[1];
    }

    /// <summary>
    /// 강화탭을 비활성화 하고 해금탭을 활성화
    /// </summary>
    public void ActivateAnimalsTap()
    {
        if (animalsTap.activeSelf || isBuyPopupActive)
        {
            return;
        }
        
        currentItemSelected = null;
        currentUpgradeSelected = null;
        
        upgradeTap.SetActive(false);
        buttonImages[1].sprite = buttonSprite[0];
        itemTap.SetActive(false);
        buttonImages[2].sprite = buttonSprite[0];
        
        animalsTap.SetActive(true);
        buttonImages[0].sprite = buttonSprite[1];
    }

    public void ActivateItemTap()
    {
        if (itemTap.activeSelf || isBuyPopupActive)
        {
            return;
        }
        
        currentSelected = null;
        currentUpgradeSelected = null;
        
        upgradeTap.SetActive(false);
        buttonImages[1].sprite = buttonSprite[0];
        animalsTap.SetActive(false);
        buttonImages[0].sprite = buttonSprite[0];
        
        itemTap.SetActive(true);
        buttonImages[2].sprite = buttonSprite[1];
    }

    #endregion
}
