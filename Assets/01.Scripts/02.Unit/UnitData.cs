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
    [SerializeField] protected GameObject psdFile;

    [Header("Stats")]
    [SerializeField] protected float maxHp;
    [SerializeField] protected float baseMoveSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float attackDelay;

    public TeamType Team => team;
    public UnitName UnitName => unitName;
    public GameObject PsdFile => psdFile;
    public float MaxHp => maxHp;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float AttackDelay => attackDelay;
}
