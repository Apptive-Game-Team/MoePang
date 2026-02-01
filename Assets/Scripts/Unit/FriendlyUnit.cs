using UnityEngine;

public class FriendlyUnit : Unit
{
    protected override void Init()
    {
        base.Init();
        spriteRenderer.flipX = true;
    }
}
