using _01.Scripts._00.Manager;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lock/Unlock 관리 클래스
/// </summary>
public class HabitatManager : SingletonObject<HabitatManager>
{
    [Header("유닛 리스트")]
    [SerializeField] private FriendlyUnitList unitList;

    //참조
    private Dictionary<FriendlyUnitData, bool> unlockDict = new();

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        unlockDict = GameManager.Instance.unitData.unlockedUnits;
    }

    /// <summary>
    /// 유닛 해금여부 판별
    /// </summary>
    public bool IsUnlocked(FriendlyUnitData unit)
    {
        return unlockDict.TryGetValue(unit, out var unlocked) && unlocked;
    }

    /// <summary>
    /// 유닛 해금
    /// </summary>
    public void Unlock(FriendlyUnitData unit)
    {
        if (unlockDict.ContainsKey(unit))
        {
            unlockDict[unit] = true;

            GameManager.Instance.unitData.unlockedUnits = unlockDict;
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
}
