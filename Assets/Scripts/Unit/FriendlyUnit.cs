using UnityEngine;

public class FriendlyUnit : Unit
{
    protected override void Init()
    {
        base.Init();
        direction = 1f;
        spriteRenderer.flipX = true;
        targetLayer = LayerMask.GetMask("Enemy");
    }
}
