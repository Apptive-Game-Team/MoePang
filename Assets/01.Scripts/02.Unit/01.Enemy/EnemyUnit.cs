using UnityEngine;

public class EnemyUnit : Unit
{
    private EnemyUnitData enemyData;

    public override void SetData(UnitData data)
    {
        base.SetData(data);

        enemyData = data as EnemyUnitData;
    }

    protected override void Init()
    {
        base.Init();
        team = TeamType.Enemy;
        direction = -1f;
        targetLayer = LayerMask.GetMask("Friendly");
    }
}
