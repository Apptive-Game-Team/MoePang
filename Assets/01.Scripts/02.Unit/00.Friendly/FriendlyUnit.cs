using UnityEngine;

public class FriendlyUnit : Unit
{
    protected override void Init()
    {
        base.Init();
        team = TeamType.Friendly;
        direction = 1f;
        transform.localScale = new Vector3(-0.2f, 0.2f, 0);
        targetLayer = LayerMask.GetMask("Enemy");
    }
}
