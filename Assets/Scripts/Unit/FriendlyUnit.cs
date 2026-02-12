using UnityEngine;

public class FriendlyUnit : Unit
{
    protected override void Init()
    {
        base.Init();
        team = TeamType.Friendly;
        direction = 1f;
        spriteRenderer.flipX = true;
        targetLayer = LayerMask.GetMask("Enemy");
    }
}
