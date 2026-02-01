using UnityEngine;

public class FriendlyUnit : Unit
{
    protected override void Init()
    {
        base.Init();
        spriteRenderer.flipX = true;
    }

    protected override void MoveState()
    {
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;
    }
}
