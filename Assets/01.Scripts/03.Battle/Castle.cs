using UnityEngine;

/// <summary>
/// 각 팀의 성
/// </summary>
public class Castle : MonoBehaviour
{
    [Header("스탯")]
    [SerializeField] protected float maxHp;
    [SerializeField] protected float currentHp;

    //프로퍼티
    public float CurrentHp => currentHp;

    private void Start()
    {
        currentHp = maxHp;
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
