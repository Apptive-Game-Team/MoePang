using UnityEngine;

public class EnemyUnit : Unit
{
    private EnemyUnitData enemyData;

    public override void SetData(UnitData data)
    {
        base.SetData(data);

        enemyData = data as EnemyUnitData;

        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        direction = -1f;
        targetLayer = LayerMask.GetMask("Friendly");
    }
}
