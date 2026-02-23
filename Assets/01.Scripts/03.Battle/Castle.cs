using UnityEngine;

/// <summary>
/// 각 팀의 성
/// </summary>
public class Castle : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private TeamType team;
    [SerializeField] protected float maxHp;
    [SerializeField] protected float currentHp;

    //프로퍼티
    public float CurrentHp => currentHp;

    public Transform GetTransform() => transform;
    public string GetName() => name;
    public TeamType GetTeam() => team;

    private void Start()
    {
        currentHp = maxHp;

        if (UnitTransformQueue.Instance != null)
        {
            UnitTransformQueue.Instance.RegisterCastle(team, this);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("타워가 아파요 ㅠ");
    }
}
