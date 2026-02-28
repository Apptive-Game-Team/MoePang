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
            return;
        }

        if (currentSelected != null)
        {
            currentSelected.Deselect();
        }

        currentSelected = clickedUI;
        currentSelected.Select();
    }
}
