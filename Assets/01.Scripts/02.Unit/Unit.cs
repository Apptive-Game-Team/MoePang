using NUnit.Framework.Constraints;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 유닛의 상태 목록
/// </summary>
public enum UnitState
{
    Move,
    Attack,
    Damage,
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

    [Header("유닛 설정")]
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected float damageDuration = 0.3f; //데미지 지속 시간

    //상태 & 참조
    protected Unit originPrefab;
    protected UnitData data;
    protected GameObject visualInstance;
    protected GameObject attackPrefab;
    protected Animator animator;
    protected UnitPool ownerPool;
    protected bool isAttacking;
    protected bool isDamaging;
    protected bool isDying;
    protected float pendingDamage;
    protected Coroutine attackRoutine;
    protected Coroutine damageRoutine;
    protected bool halfHpTriggered;
    protected Coroutine damageAnimRoutine;
    protected Tween damageTween;
    protected SpriteRenderer spriteRenderer;
    protected Vector3 originalScale;

    //프로퍼티
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

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            animator.Play("Idle", 0, 0f);
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", false);
        }

        currentState = UnitState.Attack;

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

        //비주얼(애니메이터) 설정
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder = UnitSortingManager.GetNextOrder();

        originalScale = transform.localScale;

        if (animator == null) animator = GetComponent<Animator>();

        animator.runtimeAnimatorController = data.AnimatorOverride;
    }

    /// <summary>
    /// 오브젝트 풀 지정
    /// </summary>
    public void SetPool(UnitPool pool)
    {
        this.ownerPool = pool;
    }

    public void SetOriginPrefab(Unit prefab)
    {
        originPrefab = prefab;
    }

    public Unit GetOriginPrefab()
    {
        return originPrefab;
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
            case UnitState.Damage:
                DamageState();
                break;
            case UnitState.Die:
                DieState();
                break;
        }
    }

    #region MoveState
    /// <summary>
    /// 상대를 향해 이동하는 상태
    /// </summary>
    protected virtual void MoveState()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * direction, attackRange, targetLayer);

        if (hit.collider != null)
        {
            // 2. 적을 만났다면 나를 '상대팀의 타겟 후보'로 등록 (내가 맞아야 하니까)
            // 상대방 입장에서 나는 '공격 대상'이므로 내 팀의 큐에 나를 넣음
            UTQ.Enqueue(team, this);

            animator.SetBool("Walk", false);
            currentState = UnitState.Attack;
            return;
        }

        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 맨 앞에 도착했는지 판별
    /// </summary>
    protected bool IsInFrontOf(float otherX)
    {
        return (transform.position.x * direction) > (otherX * direction);
    }
    #endregion

    #region AttackState
    /// <summary>
    /// 상대를 공격하는 상태
    /// </summary>
    protected virtual void AttackState()
    {
        if (!IsOtherInRange())
        {
            if (animator != null)
            {
                animator.SetBool("Idle", false);
                animator.SetBool("Walk", true);
            }
            
            currentState = UnitState.Move;
            return;
        }

        if (isAttacking) return;

        attackRoutine = StartCoroutine(AttackCoroutine());
    }

    /// <summary>
    /// 상대를 마주친 유닛이 공격하는 로직
    /// </summary>
    protected virtual IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("Idle", false);
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(attackDelay);

        //유닛 큐가 비어있으면 성을 공격
        TeamType enemyTeam = (team == TeamType.Friendly) ? TeamType.Enemy : TeamType.Friendly;
        IDamageable target = UTQ.Peek(enemyTeam);

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

        yield return new WaitForSeconds(0.5f - attackDelay);

        isAttacking = false;
        attackRoutine = null;
    }

    /// <summary>
    /// 상대방이 공격범위에 들어왔는지 판별
    /// </summary>
    protected virtual bool IsOtherInRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right * direction, attackRange, targetLayer);

        return hit.collider != (null);
    }
    #endregion

    #region DamageState
    /// <summary>
    /// 피격 시
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (isDying) return;

        float nextHp = currentHp - damage;

        bool triggerHalf = !halfHpTriggered && nextHp <= maxHp * 0.5f;

        if (triggerHalf)
        {
            pendingDamage = damage;
            halfHpTriggered = true;
            currentState = UnitState.Damage;
        }

        else
        {
            currentHp -= damage;

            if (damageAnimRoutine != null)
                StopCoroutine(damageAnimRoutine);

            damageAnimRoutine = StartCoroutine(DamageAnimationCoroutine());

            if (currentHp <= 0)
                currentState = UnitState.Die;
        }
    }

    protected virtual void DamageState()
    {
        if (isDamaging) return;

        damageRoutine = StartCoroutine(DamageCoroutine());
    }

    protected virtual IEnumerator DamageCoroutine()
    {
        isDamaging = true;

        StopAttack();

        if (animator != null)
            animator.SetTrigger("Damage");

        damageTween?.Kill();

        Vector3 knockback = transform.position - Vector3.right * direction * 0.5f;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(originalScale * 1.2f, 0.08f));
        seq.Join(spriteRenderer.DOFade(0.25f, 0.08f));

        seq.Append(transform.DOMove(knockback, 0.25f).SetEase(Ease.OutQuad));

        seq.Append(transform.DOScale(originalScale, 0.08f));
        seq.Join(spriteRenderer.DOFade(1f, 0.08f));

        damageTween = seq;

        yield return seq.WaitForCompletion();

        currentHp -= pendingDamage;

        if (currentHp <= 0)
            currentState = UnitState.Die;
        else
            currentState = UnitState.Attack;

        isDamaging = false;
    }

    protected IEnumerator DamageAnimationCoroutine()
    {
        damageTween?.Kill();

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(originalScale * 1.2f, 0.08f));
        seq.Join(spriteRenderer.DOFade(0.25f, 0.08f));

        seq.Append(transform.DOScale(originalScale, 0.08f));
        seq.Join(spriteRenderer.DOFade(1f, 0.08f));

        yield return seq.WaitForCompletion();
    }

    protected void StopAttack()
    {
        isAttacking = false;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }
    #endregion

    #region DieState
    /// <summary>
    /// 죽은 상태
    /// </summary>
    protected virtual void DieState()
    {
        if (isDying) return;
        StartCoroutine(DieCoroutine());
    }
    protected virtual IEnumerator DieCoroutine()
    {
        isDying = true;

        UTQ.RemoveUnit(team, this);

        StopAttack();
        isAttacking = false;
        isDamaging = false;

        damageTween?.Kill();
        transform.localScale = originalScale;
        spriteRenderer.color = Color.white;

        // 방향 반전
        direction *= -1f;
        ApplyDirectionVisual();

        if (animator != null)
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", true);
            animator.speed = 2f;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        float duration = 2f;
        float runSpeed = moveSpeed * 2f;

        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            transform.position += Vector3.right * direction * runSpeed * Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f);

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        animator.speed = 1f;
        ApplyDirectionVisual();
        isDying = false;
        ownerPool.ReturnUnit(this);
    }

    /// <summary>
    /// 방향 반전 함수
    /// </summary>
    protected void ApplyDirectionVisual()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion
}
