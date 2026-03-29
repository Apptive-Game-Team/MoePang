using UnityEngine;

[CreateAssetMenu(fileName = "FriendlyUnitData", menuName = "Unit/FriendlyUnitData")]
public class FriendlyUnitData : UnitData
{
    [Header("Friendly Only")]
    [SerializeField] private Habitat habitat;
    [SerializeField] private float unitCost;

    public Habitat Habitat => habitat;
    public float UnitCost => unitCost;
}
