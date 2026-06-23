using UnityEditor.Animations;
using UnityEngine;
using _01.Scripts._00.Manager;

/// <summary>
/// 유닛 설정 데이터
/// </summary>
public abstract class UnitData : ScriptableObject
{
    [Header("Common")]
    [SerializeField] protected TeamType team;
    [SerializeField] protected UnitName unitName;
    [SerializeField] protected int unitGrade;
    [SerializeField] protected AttackType attackType = AttackType.MeleeAttack;
    [SerializeField] protected AnimatorOverrideController animatorOverride;

    [Header("Stats")]
    [SerializeField] protected float maxHp;
    [SerializeField] protected float baseMoveSpeed;
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected float unitSize = 1f;

    [Header("Shop Data")] 
    [SerializeField] protected float unitLevel = 1f;
    [SerializeField, TextArea(3, 8)] protected string unitDescriptionText;
    

    public TeamType Team => team;
    public UnitName UnitName => unitName;
    public int UnitGrade => unitGrade;
    public AttackType AttackType => attackType;
    public AnimatorOverrideController AnimatorOverride => animatorOverride;
    public float MaxHp => maxHp + UnitLevel * 3f;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float AttackDamage => attackDamage + UnitLevel;
    public float AttackSpeed => attackSpeed;
    public float UnitSize => unitSize;
    public string UnitDescriptionText => unitDescriptionText;
    public float BaseUnitLevel => unitLevel;
    public float UnitLevel
    {
        get
        {
            if (this is FriendlyUnitData friendlyUnit)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null && gameManager.unitData != null)
                {
                    return gameManager.unitData.GetUnitLevel(friendlyUnit);
                }
            }

            return unitLevel;
        }
    }
}
