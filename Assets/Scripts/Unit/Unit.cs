using UnityEngine;

public enum UnitState
{
    Move,
    Attack,
    Die,
}

/// <summary>
/// Unit의 최상위 클래스
/// <para>기본적인 스탯, 로직 포함</para>
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("스탯")]
    [SerializeField] private float maxHp;
    [SerializeField] private float currentHp;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float bassMoveSpeed;
    [SerializeField] private float speedModifier;
    [SerializeField] private float attackRange;

    [Header("현재 상태")]
    [SerializeField] public UnitState currentState; 

    //참조
    protected SpriteRenderer spriteRenderer;

    //프로퍼티
    public float CurrentHp => currentHp;
    public float MoveSpeed => moveSpeed;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        Init();
        currentState = UnitState.Move;
    }

    protected virtual void Init()
    {
        currentHp = maxHp;
        moveSpeed = bassMoveSpeed;
    }

    private void Update()
    {
        switch (currentState)
        {
            case UnitState.Move:
                MoveState();
                break;
            case UnitState.Attack:
                AttackState();
                break;
            case UnitState.Die:
                DieState();
                break;
        }
    }

    protected virtual void MoveState()
    {
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;
    }
    protected virtual void AttackState()
    {

    }
    protected virtual void DieState()
    {

    }
}
