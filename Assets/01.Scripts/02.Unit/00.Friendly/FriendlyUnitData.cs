using _01.Scripts._00.Manager;
using UnityEngine;

[CreateAssetMenu(fileName = "FriendlyUnitData", menuName = "Unit/FriendlyUnitData")]
public class FriendlyUnitData : UnitData
{
    [Header("Friendly Only")]
    [SerializeField] private Habitat habitat;
    [SerializeField] private float unitLevel = 1f;

    public Habitat Habitat => habitat;
    public override float MaxHp => BalanceFormula.GetUnitMaxHp(maxHp, UnitLevel, UnitGrade, GetMaxStage());
    public override float AttackDamage => BalanceFormula.GetUnitAttackDamage(attackDamage, UnitLevel, UnitGrade, GetMaxStage());
    public int UnitCost => BalanceFormula.GetUnitUpgradeCost(UnitGrade, UnitLevel);
    public float BaseUnitLevel => unitLevel;
    public float UnitLevel
    {
        get
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.unitData != null)
            {
                return gameManager.unitData.GetUnitLevel(this);
            }

            return unitLevel;
        }
    }

    private int GetMaxStage()
    {
        StageManager stageManager = FindObjectOfType<StageManager>();
        return stageManager != null ? stageManager.MaxStage : 0;
    }
}
