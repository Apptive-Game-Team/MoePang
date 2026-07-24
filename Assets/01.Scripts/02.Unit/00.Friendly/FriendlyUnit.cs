using _01.Scripts._00.Manager;
using UnityEngine;
using _01.Scripts._11.HabitatMode;

public class FriendlyUnit : Unit
{
    [Header("아군 유닛 전용 데이터")]
    [SerializeField] private Habitat habitat;

    private FriendlyUnitData friendlyData;
    
    public override void SetData(UnitData data)
    {
        base.SetData(data);

        friendlyData = data as FriendlyUnitData;

        direction = 1f;
        habitat = friendlyData.Habitat;
        targetLayer = LayerMask.GetMask("Enemy");

        ApplyStageStatMultiplier();
        ApplyHabitatClearBonus();
    }

    /// <summary>
    /// 스테이지에 따른 유닛 강화단계에 따른 수치 적용
    /// </summary>
    private void ApplyStageStatMultiplier()
    {
        int currentStage = StageManager.Instance.DifficultyStage + 1;

        maxHp = BalanceFormula.GetUnitMaxHp(maxHp, friendlyData.UnitLevel, unitGrade, currentStage);
        attackDamage = BalanceFormula.GetUnitAttackDamage(attackDamage, friendlyData.UnitLevel, unitGrade, currentStage);
    }

    /// <summary>
    /// 콤보강화로 인한 스탯 증가
    /// </summary>
    private void ApplyHabitatClearBonus()
    {
        if (GameManager.Instance == null || GameManager.Instance.playData == null)
        {
            return;
        }

        int clearedStage = GetClearedHabitatStage(habitat);

        if (clearedStage <= 0)
        {
            FinalStatApply(maxHp, attackDamage);
            return;
        }

        float bonusMaxHp = clearedStage * 10f;
        float bonusAttackDamage = clearedStage * 1f;

        FinalStatApply(
            maxHp + bonusMaxHp,
            attackDamage + bonusAttackDamage
        );
    }

    private int GetClearedHabitatStage(Habitat type)
    {
        return type switch
        {
            Habitat.Meadow => GameManager.Instance.playData.MaxStages[StageType.Meadow],
            Habitat.Ocean => GameManager.Instance.playData.MaxStages[StageType.Ocean],
            Habitat.Desert => GameManager.Instance.playData.MaxStages[StageType.Desert],
            Habitat.Forest => GameManager.Instance.playData.MaxStages[StageType.Forest],
            Habitat.Polar => GameManager.Instance.playData.MaxStages[StageType.Polar],
            _ => 0
        };
    }
}
