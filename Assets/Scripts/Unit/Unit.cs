using NUnit.Framework.Constraints;
using System.Collections;
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
/// 아군진영, 적진영 타입
/// </summary>
public enum TeamType
{
    Friendly,
    Enemy
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
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float attackDelay;
    [SerializeField] protected float direction;

    [Header("탐색 설정")]
    [SerializeField] protected LayerMask targetLayer;

    [Header("현재 상태")]
    [SerializeField] protected UnitState currentState;
    [SerializeField] protected TeamType team;

    [Header("참조")]
    [SerializeField] protected GameObject attackPrefab;
    protected SpriteRenderer spriteRenderer;
    protected UnitPool ownerPool;
    protected bool isAttacking;

    protected UnitTransformQueue UTQ => UnitTransformQueue.Instance;

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
        isAttacking = false;
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
            if (UTQ.IsEmpty(team))
                UTQ.Enqueue(team, this);

            else
            {
                Unit firstUnit = UTQ.Peek(team);
                float firstX = firstUnit.transform.position.x;
                float thisX = transform.position.x;

                if (IsInFrontOf(firstX))
                {
                    UTQ.Clear(team);
                    UTQ.Enqueue(team, this);
                }
                else if (Mathf.Abs(firstX - transform.position.x) < 0.001f)
                {
                    UTQ.Enqueue(team, this);
                }
            }

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
        if (!IsOtherInRange())
        {
            currentState = UnitState.Move;
            return;
        }

        if (isAttacking) return;

        if (!isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }
    /// <summary>
    /// 죽은 상태
    /// </summary>
    protected virtual void DieState()
    {
        StopAllCoroutines();

        if (UTQ.Peek(team) == this)
        {
            UTQ.Dequeue(team);
        }

        ownerPool.ReturnUnit(this);
    }   

    /// <summary>
    /// 상대를 마주친 유닛이 공격하는 로직
    /// </summary>
    protected virtual IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        TeamType enemyTeam = (team == TeamType.Friendly) ? TeamType.Enemy : TeamType.Friendly;
        Unit target = UTQ.Peek(enemyTeam);

        //나중에 여기 애니메이션 넣기
        yield return new WaitForSeconds(attackDelay);

        if (target != null)
        {
            target.TakeDamage(attackDamage);
        }

        isAttacking = false;
    }

    /// <summary>
    /// 상대방이 공격범위에 들어왔는지 판별
    /// </summary>
    protected virtual bool IsOtherInRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right * direction, attackRange, targetLayer);

        return hit.collider != (null);
    }

    protected bool IsInFrontOf(float otherX)
    {
        return (transform.position.x * direction) > (otherX * direction);
    }

    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp < 0)
        {
            currentState = UnitState.Die;
        }
    }
}
