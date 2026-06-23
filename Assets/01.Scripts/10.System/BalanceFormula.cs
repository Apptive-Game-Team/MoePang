using UnityEngine;

public static class BalanceFormula
{
    private const int StageBonusStartMaxStage = 51;
    private const int EarlyLevelLimit = 50;
    private const int UnitUpgradeCostIncreaseAfterLevel5 = 10;

    public static int GetUnitUpgradeCost(int grade, float level)
    {
        int nextLevel = Mathf.Max(2, Mathf.FloorToInt(level) + 1);
        int level5Cost = GetUnitUpgradeCostUntilLevel5(grade, 5);

        if (nextLevel > 5)
        {
            return level5Cost + (nextLevel - 5) * UnitUpgradeCostIncreaseAfterLevel5;
        }

        return GetUnitUpgradeCostUntilLevel5(grade, nextLevel);
    }

    public static float GetUnitMaxHp(float baseHp, float level, int grade, int maxStage)
    {
        float hp = baseHp + GetUnitUpgradeValue(
            level,
            maxStage,
            GetUnitHpIncreaseBeforeStage51(grade),
            GetUnitHpIncreaseAfterStage51(grade));

        return Mathf.Ceil(hp);
    }

    public static float GetUnitAttackDamage(float baseAttackDamage, float level, int grade, int maxStage)
    {
        float attackDamage = baseAttackDamage + GetUnitUpgradeValue(
            level,
            maxStage,
            GetUnitAttackDamageIncreaseBeforeStage51(grade),
            GetUnitAttackDamageIncreaseAfterStage51(grade));

        return Mathf.Ceil(attackDamage);
    }

    public static float GetUnitHpIncreaseBeforeStage51(int grade)
    {
        return grade switch
        {
            1 => 3f,
            2 => 4f,
            3 => 5f,
            4 => 7f,
            5 => 6f,
            _ => 3f
        };
    }

    public static float GetUnitHpIncreaseAfterStage51(int grade)
    {
        return grade switch
        {
            1 => 4f,
            2 => 4f,
            3 => 5f,
            4 => 10f,
            5 => 7f,
            _ => 4f
        };
    }

    public static float GetUnitAttackDamageIncreaseBeforeStage51(int grade)
    {
        return 1f;
    }

    public static float GetUnitAttackDamageIncreaseAfterStage51(int grade)
    {
        return 1f;
    }

    private static int GetUnitUpgradeCostUntilLevel5(int grade, int nextLevel)
    {
        return grade switch
        {
            1 => nextLevel switch
            {
                2 => 30,
                3 => 40,
                4 => 50,
                5 => 60,
                _ => 30
            },
            2 => nextLevel switch
            {
                2 => 35,
                3 => 55,
                4 => 65,
                5 => 75,
                _ => 35
            },
            3 => nextLevel switch
            {
                2 => 40,
                3 => 65,
                4 => 75,
                5 => 85,
                _ => 40
            },
            4 => nextLevel switch
            {
                2 => 45,
                3 => 75,
                4 => 85,
                5 => 95,
                _ => 45
            },
            5 => nextLevel switch
            {
                2 => 50,
                3 => 85,
                4 => 95,
                5 => 105,
                _ => 50
            },
            _ => GetUnitUpgradeCostUntilLevel5(1, nextLevel)
        };
    }

    private static float GetUnitUpgradeValue(float level, int maxStage, float beforeStage51Value, float afterStage51Value)
    {
        level = Mathf.Max(0f, level);

        if (maxStage < StageBonusStartMaxStage)
        {
            return level * beforeStage51Value;
        }

        float beforeStage51Level = Mathf.Min(level, EarlyLevelLimit);
        float afterStage51Level = Mathf.Max(0f, level - EarlyLevelLimit);

        return beforeStage51Level * beforeStage51Value + afterStage51Level * afterStage51Value;
    }
}
