using UnityEngine;

public class EnemyUnit : Unit
{
    protected override void Init()
    {
        base.Init();
        team = TeamType.Enemy;
        direction = -1f;
        targetLayer = LayerMask.GetMask("Friendly");
    }

}
