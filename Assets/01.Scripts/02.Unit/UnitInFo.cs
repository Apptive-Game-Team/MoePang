using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 유닛 설정 데이터
/// </summary>
[CreateAssetMenu(fileName = "UnitInFo", menuName = "Scriptable Objects/UnitInFo")]
public class UnitInFo : ScriptableObject
{
    [Header("Unit Setting")]
    [SerializeField] TeamType team;
    [SerializeField] Habitat habitat;
    [SerializeField] UnitType unitType;
    [SerializeField] GameObject psdFile;

    [Header("유닛 설정")]
    [SerializeField] private float maxHp;
    [SerializeField] private float baseMoveSpeed; //초기 MoveSpeed
    [SerializeField] private float attackRange; //공격 사거리(근접 유닛)
    [SerializeField] private float attackDamage; //공격 데미지
    [SerializeField] private float attackDelay; //공격 속도

    //프로퍼티
    public TeamType Team => team;
    public float MaxHp => maxHp;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public float AttackDelay => attackDelay;
}
