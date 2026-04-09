using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 유닛 설정 데이터
/// </summary>
public abstract class UnitData : ScriptableObject
{
    [Header("Common")]
    [SerializeField] protected TeamType team;
    [SerializeField] protected UnitName unitName;
    [SerializeField] protected AnimatorOverrideController animatorOverride;

    [Header("Stats")]
    [SerializeField] protected float maxHp;
    [SerializeField] protected float baseMoveSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float attackSpeed;

    public TeamType Team => team;
    public UnitName UnitName => unitName;
    public AnimatorOverrideController AnimatorOverride => animatorOverride;
    public float MaxHp => maxHp;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;
}
