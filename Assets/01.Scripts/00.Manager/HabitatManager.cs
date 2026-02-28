using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitData
{
    public UnitType unitType;
    public bool isUnlocked;
}

[System.Serializable]
public class HabitatData
{
    public Habitat habitatType;
    public List<UnitData> units = new List<UnitData>();
}

public class HabitatManager : SingletonObject<HabitatManager>
{
    [SerializeField]
    private List<HabitatData> habitats = new List<HabitatData>();

    // 🔥 실제 게임에서 사용하는 런타임 Dictionary
    private Dictionary<Habitat, Dictionary<UnitType, UnitData>> habitatDict;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        habitatDict = new Dictionary<Habitat, Dictionary<UnitType, UnitData>>();

        foreach (var habitatData in habitats)
        {
            var unitDict = new Dictionary<UnitType, UnitData>();

            foreach (var unit in habitatData.units)
            {
                unitDict[unit.unitType] = unit;
            }

            habitatDict[habitatData.habitatType] = unitDict;
        }
    }
    public bool IsUnlocked(Habitat habitat, UnitType unitType)
    {
        if (habitatDict.TryGetValue(habitat, out var unitDict))
        {
            if (unitDict.TryGetValue(unitType, out var unit))
            {
                return unit.isUnlocked;
            }
        }

        return false;
    }

    public void UnlockUnit(Habitat habitat, UnitType unitType)
    {
        if (habitatDict.TryGetValue(habitat, out var unitDict))
        {
            if (unitDict.TryGetValue(unitType, out var unit))
            {
                unit.isUnlocked = true;
            }
        }
    }
}
