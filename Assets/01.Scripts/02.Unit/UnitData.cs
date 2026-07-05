using UnityEngine;

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

    [Header("Description")]
    [SerializeField, TextArea(3, 8)] protected string unitDescriptionText;
    

    public TeamType Team => team;
    public UnitName UnitName => unitName;
    public int UnitGrade => unitGrade;
    public AttackType AttackType => attackType;
    public AnimatorOverrideController AnimatorOverride => animatorOverride;
    public virtual float MaxHp => maxHp;
    public float BaseMoveSpeed => baseMoveSpeed;
    public virtual float AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;
    public float UnitSize => unitSize;
    public string UnitDescriptionText => unitDescriptionText;
}
