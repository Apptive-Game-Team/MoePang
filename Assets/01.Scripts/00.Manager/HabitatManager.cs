using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lock/Unlock 관리 클래스
/// </summary>
public class HabitatManager : SingletonObject<HabitatManager>
{
    [Header("유닛 리스트")]
    [SerializeField] private UnitList unitList;

    //참조
    private Dictionary<UnitData, bool> unlockDict = new Dictionary<UnitData, bool>();

    private void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        unlockDict.Clear();

        foreach (Habitat habitat in System.Enum.GetValues(typeof(Habitat)))
        {
            var units = unitList.GetUnits(habitat);

            if (units == null) continue;

            foreach (var unit in units)
            {
                unlockDict[unit] = false;
            }

            //첫 유닛은 기본 해금
            if (units.Count > 0)
            {
                unlockDict[units[0]] = true;
            }
        }
    }

    /// <summary>
    /// 유닛 해금여부 판별
    /// </summary>
    public bool IsUnlocked(UnitData unit)
    {
        return unlockDict.TryGetValue(unit, out var unlocked) && unlocked;
    }

    /// <summary>
    /// 유닛 해금
    /// </summary>
    public void Unlock(UnitData unit)
    {
        if (unlockDict.ContainsKey(unit))
        {
            unlockDict[unit] = true;
        }
    }

    /// <summary>
    /// 해금 가능 여부 (이전 유닛의 해금 체크
    /// </summary>
    public bool CanUnlock(UnitData unit)
    {
        var list = unitList.GetUnits(unit.Habitat);

        if (list == null) return false;

        int index = list.IndexOf(unit);

        if (index == -1) return false;

        if (index == 0) return true;

        return IsUnlocked(list[index - 1]);
    }
}
