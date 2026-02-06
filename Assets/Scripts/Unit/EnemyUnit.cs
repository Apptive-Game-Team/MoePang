using UnityEngine;

public class EnemyUnit : Unit
{
    protected override void Init()
    {
        base.Init();
        direction = -1f;
        targetLayer = LayerMask.GetMask("Friendly");
    }
}
