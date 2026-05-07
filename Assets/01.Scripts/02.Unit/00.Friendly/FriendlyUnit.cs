using _01.Scripts._00.Manager;
using UnityEngine;

public class FriendlyUnit : Unit
{
    [Header("아군 유닛 전용 데이터")]
    [SerializeField] private Habitat habitat;

    private FriendlyUnitData friendlyData;
    
    public override void SetData(UnitData data)
    {
        base.SetData(data);

        friendlyData = data as FriendlyUnitData;

        direction = 1f;

        habitat = friendlyData.Habitat;

        targetLayer = LayerMask.GetMask("Enemy");
    }
}
