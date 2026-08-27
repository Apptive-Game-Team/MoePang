using _01.Scripts._00.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lock/Unlock 관리 클래스
/// </summary>
public class HabitatManager : SingletonObject<HabitatManager>
{
    [Header("유닛 리스트")]
    [SerializeField] private FriendlyUnitList unitList;
    
    public FriendlyUnitData SelectedUnitData { get; private set; }

    //참조
    private Dictionary<FriendlyUnitData, bool> unlockDict = new();

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        unlockDict = GameManager.Instance.unitData.UnlockedUnits;
    }

    /// <summary>
    /// 유닛 해금여부 판별
    /// </summary>
    public bool IsUnlocked(FriendlyUnitData unit)
    {
        return unlockDict.TryGetValue(unit, out var unlocked) && unlocked;
    }

    public int GetUnitLevel(FriendlyUnitData unit)
    {
        return GameManager.Instance.unitData.GetUnitLevel(unit);
    }

    public void IncreaseUnitLevel(FriendlyUnitData unit)
    {
        GameManager.Instance.unitData.IncreaseUnitLevel(unit);
        GameManager.Instance.SaveUnitData();
    }

    /// <summary>
    /// 유닛 해금
    /// </summary>
    public void Unlock(FriendlyUnitData unit)
    {
        if (unlockDict.ContainsKey(unit))
        {
            unlockDict[unit] = true;

            GameManager.Instance.unitData.UnlockedUnits = unlockDict;
            GameManager.Instance.SaveUnitData();
        }
    }

    /// <summary>
    /// 해금 가능 여부 (이전 유닛의 해금 체크
    /// </summary>
    public bool CanUnlock(FriendlyUnitData unit)
    {
        var list = unitList.GetUnits(unit.Habitat);

        if (list == null) return false;

        int index = list.IndexOf(unit);

        if (index == -1) return false;

        if (index == 0) return true;

        return IsUnlocked(list[index - 1]);
    }

    public bool AreAllFinalHabitatUnitsUnlocked()
    {
        foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
        {
            List<FriendlyUnitData> units = unitList.GetUnits(habitat);

            if (units == null || units.Count == 0)
            {
                return false;
            }

            if (!IsUnlocked(units[^1]))
            {
                return false;
            }
        }

        return true;
    }
    
    public void SetSelectedUnit(FriendlyUnitData unitData)
    {
        SelectedUnitData = unitData;
    }
}
