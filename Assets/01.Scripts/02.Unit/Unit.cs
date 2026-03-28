using NUnit.Framework.Constraints;
using System.Collections;
using UnityEditor.PackageManager;
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
public class Unit : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] protected float maxHp;
    [SerializeField] protected float currentHp;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float baseMoveSpeed; //초기 MoveSpeed
    [SerializeField] protected float speedModifier; //스피드 가중치
    [SerializeField] protected float attackRange; //공격 사거리(근접 유닛)
    [SerializeField] protected float attackDamage; //공격 데미지
    [SerializeField] protected float attackDelay; //공격 속도
    [SerializeField] protected float direction; //이동, 투사체 발사 방향

    [Header("현재 상태")]
    [SerializeField] protected UnitState currentState;
    [SerializeField] protected TeamType team;

    [Header("탐색 설정")]
    [SerializeField] protected LayerMask targetLayer;

    //참조 & 프로퍼티
    protected Unit originPrefab;
    protected UnitData data;
    protected GameObject visualInstance;
    protected GameObject attackPrefab;
    protected Animator animator;
    protected UnitPool ownerPool;
    protected bool isAttacking;
    public UnitData Data => data;
    protected UnitTransformQueue UTQ => UnitTransformQueue.Instance;
    public float CurrentHp => currentHp;
    public float MoveSpeed => moveSpeed;

    public Transform GetTransform() => transform;
    public string GetName() => name;
    public TeamType GetTeam() => team;

    #region 시작 설정
    public virtual void SetData(UnitData data)
    {
        this.data = data;

        currentState = UnitState.Move;

        //피 설정
        maxHp = data.MaxHp;
        currentHp = maxHp;

        //이동속도 설정
        baseMoveSpeed = data.BaseMoveSpeed;
        moveSpeed = baseMoveSpeed;

        //공격 설정
        attackRange = data.AttackRange;
        attackDamage = data.AttackDamage;
        attackDelay = data.AttackDelay;

        //팀 설정
        team = data.Team;

        if (animator == null)
            animator = GetComponent<Animator>();

        animator.runtimeAnimatorController = data.AnimatorOverride;

        StartCoroutine(InitStateDelayed());
    }

    private IEnumerator InitStateDelayed()
    {
        yield return null;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            animator.Play("Idle", 0, 0f);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isWalking", true);
        }
    }

    /// <summary>
    /// 오브젝트 풀 지정
    /// </summary>
    public void SetPool(UnitPool pool)
    {
        this.ownerPool = pool;
    }
    #endregion

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
                IDamageable firstUnit = UTQ.Peek(team);

                float firstX = firstUnit.GetTransform().position.x;
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
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isAttacking", false);
            }
            
            currentState = UnitState.Move;
            return;
        }

        if (isAttacking) return;

        StartCoroutine(AttackCoroutine());
    }
    /// <summary>
    /// 죽은 상태
    /// </summary>
    protected virtual void DieState()
    {
        StopAllCoroutines();

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false);
        }

        if (UTQ.Peek(team) == this)
        {
            UTQ.Dequeue(team);
        }

        currentState = UnitState.Move;

        ownerPool.ReturnUnit(this);
    }   

    /// <summary>
    /// 상대를 마주친 유닛이 공격하는 로직
    /// </summary>
    protected virtual IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        TeamType enemyTeam = (team == TeamType.Friendly) ? TeamType.Enemy : TeamType.Friendly;
        IDamageable target = UTQ.Peek(enemyTeam);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", true);
        }

        yield return new WaitForSeconds(attackDelay);

        if (target != null)
        {
            Debug.Log(
                $"[{team}] {name} -> {target.GetName()} 공격 " +
                $"Damage: {attackDamage} | HP Before: {target.CurrentHp}"
            );

            target.TakeDamage(attackDamage);

            Debug.Log(
                $"[{target.GetTeam()}] {target.GetName()} HP After: {target.CurrentHp}"
            );
        }
        else
        {
            Debug.Log($"[{team}] {name} 공격했지만 타겟 없음");
        }

        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isWalking", true);
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

    /// <summary>
    /// 피격 시
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            currentState = UnitState.Die;
        }
    }

    public void SetOriginPrefab(Unit prefab)
    {
        originPrefab = prefab;
    }

    public Unit GetOriginPrefab()
    {
        return originPrefab;
    }
}
