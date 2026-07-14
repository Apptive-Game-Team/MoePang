using UnityEngine;

public static class BalanceFormula
{
    #region 스테이지 진행에 따른 아군 유닛 능력치 상승치 Fomula
    
    private const int EarlyLevelLimit = 50;
    private const int StageBonusStartMaxStage = 51;
    
    /// <summary>
    /// 아군 유닛 MaxHp Setting
    /// </summary>
    public static float GetUnitMaxHp(float baseHp, float level, int grade, int maxStage)
    {
        float hp = baseHp + GetUnitUpgradeValue(
            level,
            maxStage,
            GetUnitHpIncreaseBeforeStage51(grade),
            GetUnitHpIncreaseAfterStage51(grade));

        return Mathf.Ceil(hp);
    }

    /// <summary>
    /// 아군 유닛 Attack Damage Setting
    /// </summary>
    public static float GetUnitAttackDamage(float baseAttackDamage, float level, int grade, int maxStage)
    {
        float attackDamage = baseAttackDamage + GetUnitUpgradeValue(
            level,
            maxStage,
            GetUnitAttackDamageIncreaseBeforeStage51(grade),
            GetUnitAttackDamageIncreaseAfterStage51(grade));

        return Mathf.Ceil(attackDamage);
    }
    
    /// <summary>
    /// 51 스테이지 기준 아군 유닛 능력 상승치 변동
    /// </summary>
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
    
    /// <summary>
    /// 아군 유닛 51스테이지 이전 Unit Hp 증가량
    /// </summary>
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
    
    /// <summary>
    /// 아군 유닛 51스테이지 이후 Unit Hp 증가량
    /// </summary>
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
    
    /// <summary>
    /// 아군 유닛 51스테이지 이전 AttackDamage 증가량
    /// </summary>
    public static float GetUnitAttackDamageIncreaseBeforeStage51(int grade)
    {
        return 1f;
    }

    /// <summary>
    /// 아군 유닛 51스테이지 이후 AttackDamage 증가량
    /// </summary>
    public static float GetUnitAttackDamageIncreaseAfterStage51(int grade)
    {
        return 1f;
    }
    
    #endregion
    
    #region 스테이지 진행에 따른 적군 유닛 능력치 상승 Fomula
    
    private const float EnemyStage200HpMultiplier = 5.5f;
    private const float EnemyStage200AttackDamageMultiplier = 4f;
    private const int EnemyStageScaleBaseStage = 50;
    private const int EnemyStageScaleStartStage = 51;
    private const int EnemyStageScaleExtraStartStage = 200;
    private const int EnemyStageScaleExtraInterval = 50;
    
    /// <summary>
    /// 현재 스테이지 배율을 적용한 적 최종 MaxHp 계산
    /// </summary>
    public static float GetEnemyMaxHp(float baseHp, int currentStage)
    {
        return Mathf.Ceil(baseHp * GetEnemyHpMultiplier(currentStage));
    }
    
    /// <summary>
    /// 현재 스테이지 배율을 적용한 적 최종 Attack Damage 계산
    /// </summary>
    public static float GetEnemyAttackDamage(float baseAttackDamage, int currentStage)
    {
        return Mathf.Ceil(baseAttackDamage * GetEnemyAttackDamageMultiplier(currentStage));
    }
    
    /// <summary>
    /// 기본 체력 성장 배율에 보스 스테이지 보정 배율 곱연산
    /// </summary>
    public static float GetEnemyHpMultiplier(int currentStage)
    {
        float multiplier = GetEnemyBaseHpMultiplier(currentStage);
        return multiplier * GetEnemyBossStageMultiplier(currentStage);
    }
    
    /// <summary>
    /// 기본 공격력 성장 배율에 보스 스테이지 보정 배율 곱연산
    /// </summary>
    public static float GetEnemyAttackDamageMultiplier(int currentStage)
    {
        float multiplier = GetEnemyBaseAttackDamageMultiplier(currentStage);
        return multiplier * GetEnemyBossStageMultiplier(currentStage);
    }
    
    /// <summary>
    /// 51 ~ 200 스테이지까지 체력 배율을 선형 증가
    /// <para>201 스테이지부터는 50 스테이지마다 추가 배율 더함</para>
    /// </summary>
    private static float GetEnemyBaseHpMultiplier(int currentStage)
    {
        if (currentStage < EnemyStageScaleStartStage)
        {
            return 1f;
        }

        if (currentStage > EnemyStageScaleExtraStartStage)
        {
            return EnemyStage200HpMultiplier + GetEnemyExtraMultiplierAfterStage200(currentStage);
        }

        return 1f + 0.03f * (currentStage - EnemyStageScaleBaseStage);
    }
    
    /// <summary>
    /// 51 ~ 200 스테이지까지 공격력 배율을 선형 증가
    /// <para>201 스테이지부터는 50 스테이지마다 추가 배율 더함</para>
    /// </summary>
    private static float GetEnemyBaseAttackDamageMultiplier(int currentStage)
    {
        if (currentStage < EnemyStageScaleStartStage)
        {
            return 1f;
        }

        if (currentStage > EnemyStageScaleExtraStartStage)
        {
            return EnemyStage200AttackDamageMultiplier + GetEnemyExtraMultiplierAfterStage200(currentStage);
        }

        return 1f + 0.02f * (currentStage - EnemyStageScaleBaseStage);
    }
    
    /// <summary>
    /// 5의 배수, 10의 배수 보스 스테이지 따라 배율 적용
    /// </summary>
    private static float GetEnemyBossStageMultiplier(int currentStage)
    {
        if (currentStage < EnemyStageScaleStartStage)
        {
            return 1f;
        }

        if (currentStage % 10 == 0)
        {
            return 1.2f;
        }

        if (currentStage % 5 == 0)
        {
            return 1.1f;
        }

        return 1f;
    }
    
    /// <summary>
    /// 200 스테이지 이후, 50 스테이지마다 추가되는 후반 성장 배율 계산
    /// </summary>
    private static float GetEnemyExtraMultiplierAfterStage200(int currentStage)
    {
        return Mathf.FloorToInt((currentStage - EnemyStageScaleExtraStartStage) / (float)EnemyStageScaleExtraInterval);
    }
    #endregion
    
    #region 유닛 해금/업그레이드 비용 Fomula
    
    private const int UnitUpgradeCostIncreaseAfterLevel5 = 10;
    
    /// <summary>
    /// 단계별 유닛 해금 비용
    /// </summary>
    public static int GetUnitUnlockCost(int grade)
    {
        return grade switch
        {
            1 => 0,
            2 => 50,
            3 => 100,
            4 => 200,
            5 => 400,
            _ => 0
        };
    }
    
    /// <summary>
    /// 유닛 업그레이드 비용
    /// </summary>
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
    
    /// <summary>
    /// 5레벨 전까지의 단계별 유닛 업그레이드 비용
    /// </summary>
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

    #endregion
    
    #region 성 체력 업그레이드 스탯/비용 Fomula
    
    private const int castleHpIncreasePerLevel = 10;
    private const int castleHpBaseCost = 100;
    public static int CastleHpIncreasePerLevel => castleHpIncreasePerLevel;
    public static int CastleHpBaseCost => castleHpBaseCost;
    
    #endregion

    public static int GetEnemySpawnWeight(int grade, int currentStage)
    {
        int[] weights = GetEnemySpawnWeights(currentStage);
        int index = Mathf.Clamp(grade, 1, weights.Length) - 1;
        return weights[index];
    }

    private const int EnemySpawnWeightDynamicStartStage = 101;
    private const int EnemySpawnWeightDynamicInterval = 10;
    public static int[] GetEnemySpawnWeights(int currentStage)
    {
        int[] weights = GetEnemySpawnBaseWeights(currentStage);

        if (currentStage < EnemySpawnWeightDynamicStartStage)
        {
            return weights;
        }

        int step = (currentStage - EnemySpawnWeightDynamicStartStage) / EnemySpawnWeightDynamicInterval + 1;

        for (int i = 0; i < step; i++)
        {
            int decreased = DecreaseLowEnemySpawnWeights(weights);
            IncreaseHighEnemySpawnWeights(weights, decreased);
        }

        return weights;
    }

    private const int EnemySpawnWeightStartStage = 51;
    private static int[] GetEnemySpawnBaseWeights(int currentStage)
    {
        if (currentStage < EnemySpawnWeightStartStage)
        {
            return new[] { 100, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        if (currentStage <= 60)
        {
            return new[] { 18, 16, 14, 13, 12, 10, 8, 6, 3 };
        }

        if (currentStage <= 70)
        {
            return new[] { 14, 14, 13, 13, 12, 11, 10, 8, 5 };
        }

        if (currentStage <= 80)
        {
            return new[] { 10, 11, 11, 12, 12, 12, 12, 10, 10 };
        }

        if (currentStage <= 90)
        {
            return new[] { 7, 8, 9, 10, 11, 12, 13, 14, 16 };
        }

        return new[] { 4, 5, 6, 7, 9, 11, 14, 18, 26 };
    }

    private const int EnemySpawnMinimumWeight = 2;
    private static int DecreaseLowEnemySpawnWeights(int[] weights)
    {
        int decreased = 0;

        for (int i = 0; i < 3; i++)
        {
            if (weights[i] <= EnemySpawnMinimumWeight)
            {
                weights[i] = EnemySpawnMinimumWeight;
                continue;
            }

            weights[i]--;
            decreased++;
        }

        return decreased;
    }

    private static void IncreaseHighEnemySpawnWeights(int[] weights, int amount)
    {
        for (int i = 6; i < weights.Length && amount > 0; i++)
        {
            weights[i]++;
            amount--;
        }
    }
}
