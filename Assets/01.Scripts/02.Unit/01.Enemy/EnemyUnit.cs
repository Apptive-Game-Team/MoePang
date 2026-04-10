using UnityEngine;

public class EnemyUnit : Unit
{
    private EnemyUnitData enemyData;

    public override void SetData(UnitData data)
    {
        base.SetData(data);

        enemyData = data as EnemyUnitData;

        direction = -1f;
        ApplyDirectionVisual();

        targetLayer = LayerMask.GetMask("Friendly");
    }
}
