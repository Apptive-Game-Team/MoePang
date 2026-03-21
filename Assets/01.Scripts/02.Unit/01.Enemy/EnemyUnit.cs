using UnityEngine;

public class EnemyUnit : Unit
{
    private EnemyUnitData enemyData;

    public override void SetData(UnitData data)
    {
        base.SetData(data);

        enemyData = data as EnemyUnitData;

        if (enemyData == null)
        {
            Debug.LogError("EnemyUnit에 EnemyUnitData가 아닌 데이터가 들어옴!");
        }
    }

    protected override void Init()
    {
        base.Init();
        team = TeamType.Enemy;
        direction = -1f;
        targetLayer = LayerMask.GetMask("Friendly");
    }
}
