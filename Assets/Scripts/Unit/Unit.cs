using UnityEngine;

/// <summary>
/// 유닛의 상태 목록
/// </summary>
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
    [SerializeField] protected float maxHp;
    [SerializeField] protected float currentHp;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float bassMoveSpeed;
    [SerializeField] protected float speedModifier;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float direction;

    [Header("탐색 설정")]
    [SerializeField] protected LayerMask targetLayer;

    [Header("현재 상태")]
    [SerializeField] public UnitState currentState;

    //참조
    protected SpriteRenderer spriteRenderer;
    protected UnitPool ownerPool;

    //프로퍼티
    public float CurrentHp => currentHp;
    public float MoveSpeed => moveSpeed;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void OnEnable()
    {
        Init();
        currentState = UnitState.Move;
    }

    /// <summary>
    /// Unit 생성 시 초기화 함수
    /// </summary>
    protected virtual void Init()
    {
        currentHp = maxHp;
        moveSpeed = bassMoveSpeed;
    }

    /// <summary>
    /// 풀 지정
    /// </summary>
    /// <param name="pool"></param>
    public void SetPool(UnitPool pool)
    {
        this.ownerPool = pool;
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

    /// <summary>
    /// 상대를 향해 이동하는 상태
    /// </summary>
    protected virtual void MoveState()
    {
        if (IsOtherInRange())
        {
            currentState = UnitState.Attack;
            return;
        }

        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;
    }
    /// <summary>
    /// 상대를 공격하는 상태
    /// </summary>
    protected virtual void AttackState()
    {

    }
    /// <summary>
    /// 죽은 상태
    /// </summary>
    protected virtual void DieState()
    {
        ownerPool.ReturnUnit(this);
    }   

    /// <summary>
    /// 상대방이 공격범위에 들어왔는지 판별
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsOtherInRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right * direction, attackRange, targetLayer);

        return hit.collider != null;
    }
}
