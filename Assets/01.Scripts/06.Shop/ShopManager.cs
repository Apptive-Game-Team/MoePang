using _01.Scripts._00.Manager;
using _01.Scripts._06.Shop;
using _01.Scripts._08.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ItemData = _01.Scripts._06.Shop.ItemData;

public class ShopManager : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private List<GameObject> panels;
    [SerializeField] private Image unlockPanel;
    
    private UnitDescription unitDescription;
    private UnitInfoIcon currentSelected;
    private ItemObject currentItemSelected;
    private bool isBuyPopupActive;

    #region 초기 세팅
    private void Awake()
    {
        InitializeShopPanels();
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

            UnitInfoIcon[] ui = panel.GetComponentsInChildren<UnitInfoIcon>(true);
            foreach (var temp in ui)
            {
                temp.SetManager(this);
            }
        }
    }
    #endregion

    #region 버튼 연결 함수
    /// <summary>
    /// MainScene 복귀
    /// </summary>
    public void OnClickBack()
    {
        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.Main));
    }
    
    /// <summary>
    /// Description Scene(유닛 상세정보) 이동
    /// </summary>
    public void OnClickUnitDescription()
    {
        if (currentSelected == null)
        {
            return;
        }

        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        HabitatManager.Instance.SetSelectedUnit(currentSelected.UnitData);
        SceneManager.LoadScene(SceneInfo.GetSceneName(SceneType.UnitDescription));
    }
    
    /// <summary>
    /// 유닛 강화 버튼
    /// </summary>
    public void OnClickUnitUpgrade()
    {
        Debug.Log("ShopManager.OnClickBuy called");

        FriendlyUnitData unit = GetSelectedUnit();

        if (unit == null)
        {
            Debug.LogWarning("Buy failed: selected unit is null.");
            return;
        }

        SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
        
        int cost = GetUnitPurchaseCost(unit);
        Debug.Log($"Buy requested: {unit.UnitName}, cost: {cost}, gold: {GoldManager.Instance.Gold}");
        BuyUnitImmediately(unit);
    }
    #endregion
    
    /// <summary>
    /// 유닛이 클릭됐을 때 실행되는 함수 (UnitInfoIcon의 OnClick 다음에 실행)
    /// </summary>
    public void UnitClicked(UnitInfoIcon clickedUI)
    {
        if (currentSelected == clickedUI)
        {
            return;
        }

        if (currentSelected != null)
        {
            currentSelected.Deselect();
        }

        currentSelected = clickedUI;
        currentSelected.Select();
        HabitatManager.Instance.SetSelectedUnit(currentSelected.UnitData);
    }

    /// <summary>
    /// 유닛 해금 함수
    /// </summary>
    public void UnlockUnit(FriendlyUnitData unitData, Action refreshAction)
    {
        unlockPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{unitData.UnlockCost}G\n해금하시겠습니까?";
        
        Button yesButton = unlockPanel.transform.GetChild(2).GetComponent<Button>();

        if (GoldManager.Instance.TrySpendGold(unitData.UnlockCost))
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
                GoldManager.Instance.AdjustGold(-unitData.UnlockCost);
                HabitatManager.Instance.Unlock(unitData);
                refreshAction.Invoke();
                if (currentSelected != null)
                {
                    currentSelected.RefreshUnlockState();
                }
                unlockPanel.gameObject.SetActive(false);
            });
        }
        else
        {
            yesButton.interactable = false;
        }
        
        Button noButton = unlockPanel.transform.GetChild(3).GetComponent<Button>();
        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySFX(SFX.SFX2_ButtonClick);
            unlockPanel.gameObject.SetActive(false);
        });
        
        unlockPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 선택된 유닛 확인 (Description Scene)
    /// </summary>
    private FriendlyUnitData GetSelectedUnit()
    {
        if (currentSelected != null)
        {
            return currentSelected.UnitData;
        }

        return HabitatManager.Instance != null ? HabitatManager.Instance.SelectedUnitData : null;
    }
    
    /// <summary>
    /// 유닛 강화비용 계산 함수
    /// </summary>
    private int GetUnitPurchaseCost(FriendlyUnitData unit)
    {
        if (unit == null)
        {
            return 0;
        }

        return HabitatManager.Instance.IsUnlocked(unit) ? unit.UnitCost : unit.UnlockCost;
    }

    /// <summary>
    /// 유닛 강화 함수
    /// </summary>
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
            return;
        }

        if (!GoldManager.Instance.TrySpendGold(cost))
        {
            Debug.LogWarning($"Buy failed: not enough gold. Current: {GoldManager.Instance.Gold}, Cost: {cost}");
            return;
        }

        GoldManager.Instance.AdjustGold(-cost);

        if (isUnlocked)
        {
            HabitatManager.Instance.IncreaseUnitLevel(unit);
            Debug.Log($"Level up succeeded: {unit.UnitName}, level: {unit.UnitLevel}, remaining gold: {GoldManager.Instance.Gold}");
            RefreshUnitDescription();
            return;
        }

        HabitatManager.Instance.Unlock(unit);
        Debug.Log($"Buy succeeded: {unit.UnitName}, remaining gold: {GoldManager.Instance.Gold}");

        if (currentSelected != null)
        {
            currentSelected.RefreshUnlockState();
        }

        RefreshUnitDescription();
    }

    /// <summary>
    /// Unit Description 텍스트 최신화
    /// </summary>
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
}
