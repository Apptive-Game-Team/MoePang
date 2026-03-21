using UnityEngine;

public class FriendlyUnit : Unit
{
    private FriendlyUnitData friendlyData;

    public override void SetData(UnitData data)
    {
        base.SetData(data);

        friendlyData = data as FriendlyUnitData;
    }

    protected override void Init()
    {
        base.Init();
        team = TeamType.Friendly;
        direction = 1f;

        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;

        targetLayer = LayerMask.GetMask("Enemy");
    }
}
