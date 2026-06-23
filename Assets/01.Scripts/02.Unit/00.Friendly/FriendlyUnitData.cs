using UnityEngine;

[CreateAssetMenu(fileName = "FriendlyUnitData", menuName = "Unit/FriendlyUnitData")]
public class FriendlyUnitData : UnitData
{
    private const int DefaultUnitCost = 100;

    [Header("Friendly Only")]
    [SerializeField] private Habitat habitat;

    public Habitat Habitat => habitat;
    public int UnitCost => DefaultUnitCost;
}
