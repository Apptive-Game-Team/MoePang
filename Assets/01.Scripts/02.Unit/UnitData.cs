using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 유닛 설정 데이터
/// </summary>
[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Unit Setting")]
    [SerializeField] private TeamType team;
    [SerializeField] private Habitat habitat;
    [SerializeField] private UnitName unitName;
    [SerializeField] private GameObject psdFile;
    [SerializeField] private float unitCost;

    [Header("유닛 설정")]
    [SerializeField] private float maxHp;
    [SerializeField] private float baseMoveSpeed; //초기 MoveSpeed
    [SerializeField] private float attackRange; //공격 사거리(근접 유닛)
    [SerializeField] private float attackDamage; //공격 데미지
    [SerializeField] private float attackDelay; //공격 속도

    //프로퍼티
    public TeamType Team => team;
    public Habitat Habitat => habitat;
    public UnitName UnitName => unitName;
    public GameObject PsdFile => psdFile;
    public float UnitCost => unitCost;
    public float MaxHp => maxHp;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float AttackDelay => attackDelay;
}
