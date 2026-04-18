using _01.Scripts._00.Manager;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : SingletonObject<UpgradeManager>
{
    private Dictionary<UpgradeData, int> upgradeLevels = new();

    public int GetLevel(UpgradeData data)
    {
        if (!upgradeLevels.ContainsKey(data))
            upgradeLevels[data] = 0;

        return upgradeLevels[data];
    }

    public bool CanUpgrade(UpgradeData data)
    {
        return GetLevel(data) < data.MaxLevel;
    }

    public int GetCost(UpgradeData data)
    {
        int level = GetLevel(data);
        return data.BaseCost * (level + 1);
    }

    public void Upgrade(UpgradeData data)
    {
        if (!CanUpgrade(data)) return;

        upgradeLevels[data]++;

        GameManager.Instance.unitData.castleLevels = upgradeLevels;
        GameManager.Instance.SaveUnitData();

        ApplyUpgrade(data);
    }

    private void ApplyUpgrade(UpgradeData data)
    {
        if (data.UpgradeType == UpgradeType.CastleHP)
        {
            CastleManager.Instance.MaxHp += data.IncreasePerLevel;
        }
    }
}