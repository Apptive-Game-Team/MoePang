using UnityEngine;

[CreateAssetMenu(fileName = "FriendlyUnitData", menuName = "Unit/FriendlyUnitData")]
public class FriendlyUnitData : UnitData
{
    [Header("Friendly Only")]
    [SerializeField] private Habitat habitat;

    public Habitat Habitat => habitat;
    public int UnitCost => BalanceFormula.GetUnitUpgradeCost(UnitGrade, UnitLevel);
}
