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
    [SerializeField] private GameObject animalsTap;
    [SerializeField] private GameObject upgradeTap;
    [SerializeField] private GameObject itemTap;
    [SerializeField] private Image[] buttonImages;
    [SerializeField] private Sprite[] buttonSprite;

    [Header("버튼")]
    [SerializeField] private Button animalsTapButton;
    [SerializeField] private Button upgradeTapButton;

    [Header("구매 팝업")]
    [SerializeField] private GameObject buyPopup;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("설명 UI")]
    [SerializeField] private UnitDescription unitDescription;

    private ShopUI currentSelected;
    private UpgradeUI currentUpgradeSelected;
    private ItemObject currentItemSelected;
    private bool isBuyPopupActive;

    private void Awake()
    {
        InitializeShopPanels();
    }

    private void Start()
    {
        EnsureBuyButtonText();

        if (animalsTap != null)
        {
            animalsTap.SetActive(true);
        }

        if (upgradeTap != null)
        {
            upgradeTap.SetActive(false);
        }

        if (itemTap != null)
        {
            itemTap.SetActive(false);
        }

        if (buyPopup != null)
        {
            buyPopup.SetActive(false);
        }

        UpdateBuyButtonText();
        OnGoldChanged();
    }

    private void OnEnable()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += OnGoldChanged;
        }
    }

    private void OnDisable()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged -= OnGoldChanged;
        }
    }

    private void InitializeShopPanels()
    {
        if (panels == null)
        {
            return;
        }

        foreach (var panel in panels)
        {
            if (panel == null)
            {
                continue;
            }

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

            if (itemData == null)
            {
                continue;
            }

            ItemObject[] itemUI = panel.GetComponentsInChildren<ItemObject>(true);
            foreach (var temp in itemUI)
            {
                var data = itemData.items.FirstOrDefault(info => info.type == temp.type);
                if (data != null)
                {
                    temp.Init(data);
                }
            }
        }
    }

    private void OnGoldChanged()
    {
        if (goldText != null)
        {
            goldText.text = $"{GoldManager.Instance.Gold}";
        }
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
    }

    public void OnClickUnitDescription()
    {
        if (currentSelected == null)
        {
            return;
        }

        HabitatManager.Instance.SetSelectedUnit(currentSelected.UnitData);
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.UnitDescription));
    }

    public void OnClickUnit(ShopUI clickedUI)
    {
        if (isBuyPopupActive)
        {
            return;
        }

        if (currentSelected == clickedUI)
        {
            currentSelected.Deselect();
            currentSelected = null;
            HabitatManager.Instance.SetSelectedUnit(null);
            UpdateBuyButtonText();
            return;
        }

        if (currentSelected != null)
        {
            currentSelected.Deselect();
        }

        currentSelected = clickedUI;
        currentSelected.Select();
        HabitatManager.Instance.SetSelectedUnit(currentSelected.UnitData);

        UpdateBuyButtonText();
    }

    public void OnClickUpgrade(UpgradeUI clickedUI)
    {
        if (clickedUI == null)
        {
            return;
        }

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

    public void OnClickItem(ItemObject clickedUI)
    {
        currentItemSelected = currentItemSelected == clickedUI ? null : clickedUI;
        UpdateBuyButtonText();
    }

    private void UpdateBuyButtonText()
    {
        if (buyButtonText == null)
        {
            return;
        }

        FriendlyUnitData unit = GetSelectedUnit();

        if (unit == null)
        {
            buyButtonText.text = "Buy";
            return;
        }

        if (HabitatManager.Instance.IsUnlocked(unit))
        {
            buyButtonText.text = "Level Up";
            return;
        }

        if (!HabitatManager.Instance.CanUnlock(unit))
        {
            buyButtonText.text = "잠겨있음";
            return;
        }

        buyButtonText.text = "Buy";
    }

    public void OnClickBuy()
    {
        Debug.Log("ShopManager.OnClickBuy called");

        FriendlyUnitData unit = GetSelectedUnit();

        if (unit == null)
        {
            Debug.LogWarning("Buy failed: selected unit is null.");
            SetBuyButtonText("선택 없음");
            return;
        }

        int cost = GetUnitPurchaseCost(unit);
        Debug.Log($"Buy requested: {unit.UnitName}, cost: {cost}, gold: {GoldManager.Instance.Gold}");
        BuyUnitImmediately(unit);
    }

    public void ConfirmPurchase()
    {
        FriendlyUnitData unit = GetSelectedUnit();

        if (unit != null)
        {
            BuyUnitImmediately(unit);
            ClosePopup();
            return;
        }

        ClosePopup();
    }

    public void ClosePopup()
    {
        if (buyPopup != null)
        {
            isBuyPopupActive = false;
            buyPopup.SetActive(false);
        }
    }

    private FriendlyUnitData GetSelectedUnit()
    {
        if (currentSelected != null)
        {
            return currentSelected.UnitData;
        }

        return HabitatManager.Instance != null ? HabitatManager.Instance.SelectedUnitData : null;
    }

    private void BuyUnitImmediately(FriendlyUnitData unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("Buy failed: unit is null.");
            return;
        }

        bool isUnlocked = HabitatManager.Instance.IsUnlocked(unit);
        int cost = GetUnitPurchaseCost(unit);

        if (!isUnlocked && !HabitatManager.Instance.CanUnlock(unit))
        {
            Debug.LogWarning($"Buy failed: {unit.UnitName} cannot be unlocked yet.");
            SetBuyButtonText("앞에꺼사.");
            return;
        }

        if (!GoldManager.Instance.TrySpendGold(cost))
        {
            Debug.LogWarning($"Buy failed: not enough gold. Current: {GoldManager.Instance.Gold}, Cost: {cost}");
            SetBuyButtonText("돈없엉");
            return;
        }

        GoldManager.Instance.AdjustGold(-cost);

        if (isUnlocked)
        {
            HabitatManager.Instance.IncreaseUnitLevel(unit);
            Debug.Log($"Level up succeeded: {unit.UnitName}, level: {unit.UnitLevel}, remaining gold: {GoldManager.Instance.Gold}");
            RefreshUnitDescription();
            SetBuyButtonText("Level Up");
            return;
        }

        HabitatManager.Instance.Unlock(unit);
        Debug.Log($"Buy succeeded: {unit.UnitName}, remaining gold: {GoldManager.Instance.Gold}");

        if (currentSelected != null)
        {
            currentSelected.RefreshUnlockState();
        }

        RefreshUnitDescription();
        SetBuyButtonText("Level Up");
    }

    private int GetUnitPurchaseCost(FriendlyUnitData unit)
    {
        if (unit == null)
        {
            return 0;
        }

        return HabitatManager.Instance.IsUnlocked(unit) ? unit.UnitCost : unit.UnlockCost;
    }

    private void RefreshUnitDescription()
    {
        if (unitDescription == null)
        {
            unitDescription = FindObjectOfType<UnitDescription>();
        }

        if (unitDescription != null)
        {
            unitDescription.RefreshDescription();
        }
    }

    private void SetBuyButtonText(string text)
    {
        EnsureBuyButtonText();

        if (buyButtonText != null)
        {
            buyButtonText.text = text;
        }
        else
        {
            Debug.LogWarning($"buyButtonText is not assigned. Message was: {text}");
        }
    }

    private void EnsureBuyButtonText()
    {
        if (buyButtonText != null)
        {
            return;
        }

        GameObject buyButtonObject = GameObject.Find("UnitUpgradeButton");
        if (buyButtonObject != null)
        {
            buyButtonText = buyButtonObject.GetComponent<TextMeshProUGUI>();
        }
    }

    public void ActivateUpgradeTap()
    {
        if (upgradeTap == null || upgradeTap.activeSelf || isBuyPopupActive)
        {
            return;
        }

        currentItemSelected = null;
        currentSelected = null;

        if (animalsTap != null)
        {
            animalsTap.SetActive(false);
        }

        if (itemTap != null)
        {
            itemTap.SetActive(false);
        }

        SetButtonSprite(0, 0);
        SetButtonSprite(2, 0);

        upgradeTap.SetActive(true);
        SetButtonSprite(1, 1);
    }

    public void ActivateAnimalsTap()
    {
        if (animalsTap == null || animalsTap.activeSelf || isBuyPopupActive)
        {
            return;
        }

        currentItemSelected = null;
        currentUpgradeSelected = null;

        if (upgradeTap != null)
        {
            upgradeTap.SetActive(false);
        }

        if (itemTap != null)
        {
            itemTap.SetActive(false);
        }

        SetButtonSprite(1, 0);
        SetButtonSprite(2, 0);

        animalsTap.SetActive(true);
        SetButtonSprite(0, 1);
    }

    public void ActivateItemTap()
    {
        if (itemTap == null || itemTap.activeSelf || isBuyPopupActive)
        {
            return;
        }

        currentSelected = null;
        currentUpgradeSelected = null;

        if (upgradeTap != null)
        {
            upgradeTap.SetActive(false);
        }

        if (animalsTap != null)
        {
            animalsTap.SetActive(false);
        }

        SetButtonSprite(1, 0);
        SetButtonSprite(0, 0);

        itemTap.SetActive(true);
        SetButtonSprite(2, 1);
    }

    private void SetButtonSprite(int imageIndex, int spriteIndex)
    {
        if (buttonImages == null || buttonSprite == null)
        {
            return;
        }

        if (imageIndex < 0 || imageIndex >= buttonImages.Length || spriteIndex < 0 || spriteIndex >= buttonSprite.Length)
        {
            return;
        }

        if (buttonImages[imageIndex] != null)
        {
            buttonImages[imageIndex].sprite = buttonSprite[spriteIndex];
        }
    }
}
