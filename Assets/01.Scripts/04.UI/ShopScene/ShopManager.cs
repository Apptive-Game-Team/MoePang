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

    [Header("상태")]
    [SerializeField] private bool isUnitClicked = false;
    [SerializeField] private ShopUI currentSelected;

    private List<ShopUI> allShopUI = new List<ShopUI>();

    private void Awake()
    {
        foreach (var panel in panels)
        {
            ShopUI[] ui = panel.GetComponentsInChildren<ShopUI>(true);

            foreach (var temp in ui)
            {
                temp.SetManager(this);
            }

            allShopUI.AddRange(ui);
        }
    }

    private void Start()
    {
        goldText.text = $"Gold : {GoldManager.Instance.Gold}";
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

    private void UpdateBuyButtonText()
    {
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
}
