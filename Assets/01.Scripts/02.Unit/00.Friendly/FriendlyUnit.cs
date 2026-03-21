using UnityEngine;

public class FriendlyUnit : Unit
{
    private FriendlyUnitData friendlyData;

    public override void SetData(UnitData data)
    {
        base.SetData(data);

        friendlyData = data as FriendlyUnitData;

        if (friendlyData == null)
        {
            Debug.LogError("FriendlyUnit에 FriendlyUnitData가 아닌 데이터가 들어옴!");
        }
    }

    protected override void Init()
    {
        base.Init();
        team = TeamType.Friendly;
        direction = 1f;
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1f, 1f, 1f));
        targetLayer = LayerMask.GetMask("Enemy");
    }
}
