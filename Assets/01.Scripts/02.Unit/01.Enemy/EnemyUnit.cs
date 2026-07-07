using UnityEngine;

public class EnemyUnit : Unit
{
    private EnemyUnitData enemyData;

    public override void SetData(UnitData data)
    {
        base.SetData(data);

        enemyData = data as EnemyUnitData;

        ApplyStageStatMultiplier();

        direction = -1f;

        targetLayer = LayerMask.GetMask("Friendly");
    }

    private void ApplyStageStatMultiplier()
    {
        if (enemyData == null || StageManager.Instance == null)
        {
            return;
        }

        int currentStage = StageManager.Instance.DifficultyStage + 1;
        float stageMaxHp = BalanceFormula.GetEnemyMaxHp(maxHp, currentStage);
        float stageAttackDamage = BalanceFormula.GetEnemyAttackDamage(attackDamage, currentStage);

        ApplyBaseHpAndAttackDamage(stageMaxHp, stageAttackDamage);
    }
}
