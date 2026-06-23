using UnityEngine;

public static class BalanceFormula
{
    private const int DefaultUnitUpgradeCost = 100;
    private const int StageBonusStartMaxStage = 51;
    private const int EarlyLevelLimit = 50;

    public static int GetUnitUpgradeCost(int grade, float level)
    {
        return DefaultUnitUpgradeCost;
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
