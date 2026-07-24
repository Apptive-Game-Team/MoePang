using _01.Scripts._00.Manager;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

#region Enums
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
/// 유닛의 공격 방법
/// </summary>
public enum AttackType
{
    MeleeAttack,
    RangeAttack,
    TripleAttack,
    LongMeleeAttack,
}

/// <summary>
/// 아군진영, 적진영 타입
/// </summary>
public enum TeamType
{
    Friendly,
    Enemy
}
#endregion

/// <summary>
/// Unit의 최상위 클래스
/// <para>기본적인 스탯, 로직 포함</para>
/// </summary>
public class Unit : MonoBehaviour, IDamageable
{
    [Header("유닛 스탯")] 
    [SerializeField] protected int unitGrade;
    [SerializeField] protected float maxHp;
    [SerializeField] protected float currentHp;
    [SerializeField] protected float moveSpeed; //이동 속도
    [SerializeField] protected float attackDamage; //공격 데미지
    [SerializeField] protected float attackSpeed; //공격 속도 (1초에 몇 번 공격하는지)
    [SerializeField] protected float unitSize; //유닛 크기
    private float _originMoveSpeed;
    private float _originAttackDamage;
    private float _originAttackSpeed;

    [Header("유닛 설정")]
    [SerializeField] protected UnitState currentState;
    [SerializeField] protected TeamType team;
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] protected AttackType attackType = AttackType.MeleeAttack;
    [SerializeField] protected float direction; //이동 방향
    [SerializeField] protected float damageDuration = 0.3f; //데미지 지속 시간
    
    [Header("Long Melee Attack")]
    [SerializeField] protected RangeAttackPrefab attackProjectilePrefab;
    
    private List<Buff> _activeBuffs = new();
    
    //외부 참조
    protected UnitPool ownerPool;

    //내부 컴포넌트
    protected Unit originPrefab;
    protected UnitData data;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    
    //북극 여우 공격
    private float articFoxJumpHeight = 0.4f;
    private float articFoxLandingOffset = 0.35f;
    private float articFoxReturnDuration = 0.2f;
    private bool isArticFoxAttackMoving;
    private Vector3 articFoxAttackOriginPosition;
    private Tween articFoxAttackTween;

    //상태 관리
    protected bool isAttacking;
    protected bool halfHpTriggered;
    protected bool isHitEffect;
    protected bool isDamaging;
    protected bool isDying;

    //수치 / 로직 관리
    protected Vector3 originalScale;
    protected float attackRange = 1f;
    protected float pendingDamage;
    protected Tween damageTween;
    protected Coroutine attackRoutine;
    protected Coroutine damageRoutine;
    protected Coroutine damageAnimRoutine;

    //프로퍼티
    public UnitData Data => data;
    protected UnitTransformQueue UTQ => UnitTransformQueue.Instance;
    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public Transform GetTransform() => transform;
    public string GetName() => name;
    public TeamType GetTeam() => team;

    #region 시작 설정
    public virtual void SetData(UnitData data)
    {
        this.data = data;

        SetBaseStat();
        SetVisual();

        UTQ.ResetAndInsert(this);
        BuffManager.Instance.RegisterUnit(this);

        ChangeState(UnitState.Move, true);
    }

    /// <summary>
    /// Base Data 설정
    /// </summary>
    protected virtual void SetBaseStat()
    {
        team = data.Team;

        maxHp = data.MaxHp;
        attackDamage = data.AttackDamage;

        unitGrade = data.UnitGrade;
        attackType = data.AttackType;
        unitSize = data.UnitSize;

        moveSpeed = data.BaseMoveSpeed;
        attackSpeed = data.AttackSpeed;
        attackRange = unitSize;

        if (data.RangeAttackPrefab != null)
            attackProjectilePrefab = data.RangeAttackPrefab;
    }

    /// <summary>
    /// Sprite/Animation Visual Setting
    /// </summary>
    protected virtual void SetVisual()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        originalScale = transform.localScale;
        spriteRenderer.sortingOrder = GetSortingOrderByY();
        animator.runtimeAnimatorController = data.AnimatorOverride;

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        
        UnitShadow shadow = GetComponentInChildren<UnitShadow>(true);
        if (shadow != null)
        {
            shadow.Refresh();
        }
    }

    private int GetSortingOrderByY()
    {
        const int baseOrder = 1000;
        const int precision = 100;

        return baseOrder - Mathf.RoundToInt(transform.position.y * precision);
    }

    /// <summary>
    /// 최종적으로 적용되는 공격력/방어력
    /// </summary>
    protected void FinalStatApply(float newMaxHp, float newAttackDamage)
    {
        maxHp = newMaxHp;
        currentHp = maxHp;
        attackDamage = newAttackDamage;

        _originMoveSpeed = moveSpeed;
        _originAttackDamage = attackDamage;
        _originAttackSpeed = attackSpeed;
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

    protected void ChangeState(UnitState newState, bool force = false)
    {
        if (!force && currentState == newState) return;

        currentState = newState;

        animator.speed = 1f;

        switch (currentState)
        {
            case UnitState.Move:
                animator.SetBool("Walk", true);
                animator.SetBool("Idle", false);
                break;

            case UnitState.Attack:
                animator.SetBool("Walk", false);
                animator.SetBool("Idle", false);
                break;

            case UnitState.Damage:
                animator.SetBool("Walk", false);
                animator.SetBool("Idle", false);
                animator.SetTrigger("Damage");
                break;

            case UnitState.Die:
                animator.speed = 2f;

                animator.SetBool("Walk", true);
                animator.SetBool("Idle", false);
                break;
        }
    }

    #region MoveState
    /// <summary>
    /// 상대를 향해 이동하는 상태
    /// </summary>
    protected virtual void MoveState()
    {
        if (animator != null && !animator.GetBool("Walk"))
        {
            animator.SetBool("Walk", true);
            animator.SetBool("Idle", false);
        }
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * direction, attackRange, targetLayer);

        if (hit.collider != null)
        {
            UTQ.Insert(this);
            ChangeState(UnitState.Attack);
            return;
        }

        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;
    }
    #endregion

    #region AttackState
    /// <summary>
    /// 상대를 공격하는 상태
    /// </summary>
    protected virtual void AttackState()
    {
        if (isAttacking) return;

        if (!IsOtherInRange())
        {
            ChangeState(UnitState.Move);
            return;
        }

        attackRoutine = StartCoroutine(AttackCoroutine());
    }

    /// <summary>
    /// 상대를 마주친 유닛이 공격하는 로직
    /// </summary>
    protected virtual IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        float segment = 1f / attackSpeed / 3f;

        if (data.UnitName == UnitName.ArticFox)
        {
            yield return StartCoroutine(ArticFoxAttackCoroutine(segment));
        }
        else
        {
            animator.speed = attackSpeed;

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            yield return new WaitForSeconds(segment);

            AttackByType();

            yield return new WaitForSeconds(segment);

            if (animator != null)
            {
                animator.SetBool("Idle", true);
                animator.SetBool("Walk", false);
            }

            yield return new WaitForSeconds(segment);
        }

        isAttacking = false;
        attackRoutine = null;
    }

    /// <summary>
    /// 상대방이 공격범위에 들어왔는지 판별
    /// </summary>
    /*protected virtual bool IsOtherInRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right * direction, attackRange, targetLayer);

        return hit.collider != (null);
    }*/
    protected virtual bool IsOtherInRange()
    {
        return GetRaycastTarget() != null;
    }
    
    /// <summary>
    /// 공격 타입에 따른 공격 방법
    /// </summary>
    protected virtual void AttackByType()
    {
        switch (attackType)
        {
            case AttackType.MeleeAttack:
                MeleeAttack();
                break;
            
            case AttackType.LongMeleeAttack:
                LongMeleeAttack();
                break;

            case AttackType.RangeAttack:
                RangeAttack();
                break;

            case AttackType.TripleAttack:
                TripleAttack();
                break;

            default:
                Debug.LogWarning("이런 공격은 없어");
                MeleeAttack();
                break;
        }
    }

    /// <summary>
    /// 근접 공격
    /// </summary>
    /*protected virtual void MeleeAttack()
    {
        TeamType enemyTeam = (team == TeamType.Friendly) ? TeamType.Enemy : TeamType.Friendly;
        IDamageable target = UTQ.Peek(enemyTeam);
        if (target == null)
        {
            Debug.Log($"[AttackTarget] attacker={name}, attackerTeam={team}, target=null");
        }
        else
        {
            Transform targetTransform = target.GetTransform();
            Collider2D targetCollider = targetTransform != null
                ? targetTransform.GetComponent<Collider2D>()
                : null;

            Vector2 targetPoint = targetTransform != null
                ? targetTransform.position
                : Vector2.zero;

            if (targetCollider != null)
                targetPoint = targetCollider.ClosestPoint(transform.position);

            float forwardDistance = (targetPoint.x - transform.position.x) * direction;
            bool inRange = forwardDistance >= 0f && forwardDistance <= attackRange;

            Debug.Log(
                $"[AttackTarget] " +
                $"attacker={name}, attackerTeam={team}, attackerPos={transform.position}, direction={direction}, " +
                $"target={target.GetName()}, targetType={target.GetType().Name}, targetTeam={target.GetTeam()}, " +
                $"targetHp={target.CurrentHp}, targetPos={(targetTransform != null ? targetTransform.position.ToString() : "null")}, " +
                $"collider={(targetCollider != null ? targetCollider.GetType().Name : "null")}, " +
                $"closestPoint={targetPoint}, forwardDistance={forwardDistance}, attackRange={attackRange}, inRange={inRange}"
            );
        }

        if (target != null)
        {
            if (!IsTargetInAttackRange(target))
            {
                Debug.Log("1번 오류");
                return;
            }
            
            target.TakeDamage(attackDamage);
        }

        else
        {
            Debug.Log($"[{team}] {name} 공격했지만 타겟 없음");
        }
    }*/
    protected virtual void MeleeAttack()
    {
        IDamageable target = GetRaycastTarget();

        if (target == null)
        {
            TeamType enemyTeam = team == TeamType.Friendly ? TeamType.Enemy : TeamType.Friendly;
            target = UTQ.Peek(enemyTeam);
        }

        if (target == null)
        {
            Debug.Log($"[{team}] {name} 공격했지만 대상 없음");
            return;
        }

        if (!IsTargetInAttackRange(target))
        {
            Debug.Log($"[AttackBlocked] target={target.GetName()}, type={target.GetType().Name}");
            return;
        }

        target.TakeDamage(attackDamage);
    }

    private IDamageable GetRaycastTarget()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right * direction,
            attackRange,
            targetLayer
        );

        if (hit.collider == null)
            return null;

        IDamageable target = hit.collider.GetComponentInParent<IDamageable>();

        if (target == null)
            return null;

        if (target.GetTeam() == team)
            return null;

        return target;
    }

    private bool IsTargetInAttackRange(IDamageable target)
    {
        Vector2 targetPoint = target.GetTransform().position;
        Collider2D targetCollider = target.GetTransform().GetComponent<Collider2D>();

        if (targetCollider != null)
            targetPoint = targetCollider.ClosestPoint(transform.position);

        float forwardDistance = (targetPoint.x - transform.position.x) * direction;

        return forwardDistance >= 0f && forwardDistance <= attackRange;
    }
    
    protected virtual void LongMeleeAttack()
    {
        IDamageable target = GetRaycastTarget();

        if (target == null)
        {
            TeamType enemyTeam = team == TeamType.Friendly ? TeamType.Enemy : TeamType.Friendly;
            target = UTQ.Peek(enemyTeam);
        }

        if (target == null) return;
        if (!IsTargetInAttackRange(target)) return;

        FireProjectile(target);
    }
    
    protected virtual void FireProjectile(IDamageable target)
    {
        if (attackProjectilePrefab == null) return;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        float unitHeight = spriteRenderer != null
            ? spriteRenderer.bounds.size.y
            : unitSize;

        Vector3 spawnPosition = transform.position + Vector3.up * (unitHeight * 0.5f);

        RangeAttackPrefab projectile = Instantiate(
            attackProjectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        Vector2 fireDirection = Vector2.right * direction;

        projectile.Init(
            attackDamage,
            team,
            targetLayer,
            fireDirection
        );
    }
    
    /// <summary>
    /// 원거리 공격
    /// </summary>
    protected virtual void RangeAttack()
    {
        Vector2 boxCenter = GetAttackBoxCenter();
        Vector2 boxSize = GetAttackBoxSize();

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, targetLayer);

        if (hits.Length == 0)
            return;

        foreach (Collider2D hit in hits)
        {
            IDamageable target = hit.GetComponentInParent<IDamageable>();

            if (target == null) continue;
            if (target.GetTeam() == team) continue;

            target.TakeDamage(attackDamage);
        }
    }
    
    

    protected virtual void TripleAttack()
    {
        TeamType enemyTeam = (team == TeamType.Friendly) ? TeamType.Enemy : TeamType.Friendly;
        List<IDamageable> targets = UTQ.PeekTargets(enemyTeam, 3);

        foreach (IDamageable target in targets)
        {
            if (target == null) continue;
            if (target is Castle && !IsTargetInAttackRange(target)) continue;

            target.TakeDamage(attackDamage);
        }
    }
    
    protected virtual Vector2 GetAttackBoxCenter()
    {
        return transform.position + Vector3.right * direction * attackRange * 0.5f;
    }

    protected virtual Vector2 GetAttackBoxSize()
    {
        return new Vector2(attackRange, unitSize);
    }
    
    /// <summary>
    /// 북극여우 공격
    /// </summary>
    private IEnumerator ArticFoxAttackCoroutine(float segment)
    {
        IDamageable target = GetRaycastTarget();

        if (target == null)
        {
            TeamType enemyTeam = team == TeamType.Friendly ? TeamType.Enemy : TeamType.Friendly;
            target = UTQ.Peek(enemyTeam);
        }

        if (target == null)
            yield break;

        Vector3 originPosition = transform.position;
        articFoxAttackOriginPosition = originPosition;
        isArticFoxAttackMoving = true;

        Vector3 targetPosition = target.GetTransform().position;

        //float landingX = targetPosition.x - direction * articFoxLandingOffset;
        float landingX = originPosition.x;
        Vector3 landingPosition = new Vector3(
            landingX,
            originPosition.y,
            originPosition.z
        );

        float middleX = (originPosition.x + landingPosition.x) * 0.5f;
        Vector3 jumpPeakPosition = new Vector3(
            middleX,
            originPosition.y + articFoxJumpHeight,
            originPosition.z
        );

        // Attack 애니메이션 0~10프레임: 하늘로 뜸
        if (animator != null)
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", false);
            animator.speed = attackSpeed * 0.5f;
            animator.Play("Attack", 0, 0f);
            animator.Update(0f);
        }

        articFoxAttackTween = transform
            .DOMove(jumpPeakPosition, segment)
            .SetEase(Ease.OutQuad);

        yield return articFoxAttackTween.WaitForCompletion();

        // Attack 애니메이션 10~20프레임: 내려가며 돌진
        if (animator != null)
        {
            animator.Play("Attack", 0, 0.5f);
            animator.Update(0f);
        }

        articFoxAttackTween = transform
            .DOMove(landingPosition, segment)
            .SetEase(Ease.InQuad);

        yield return articFoxAttackTween.WaitForCompletion();

        // 착지 시점에 공격 판정
        AttackByType();

        // 잠깐 Idle로 복귀
        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool("Idle", true);
            animator.SetBool("Walk", false);
        }

        articFoxAttackTween = transform
            .DOMove(originPosition, articFoxReturnDuration)
            .SetEase(Ease.OutQuad);

        yield return articFoxAttackTween.WaitForCompletion();

        isArticFoxAttackMoving = false;
        articFoxAttackTween = null;
    }
    
    private void ResetArticFoxAttackPositionIfNeeded()
    {
        if (!isArticFoxAttackMoving) return;

        if (articFoxAttackTween != null && articFoxAttackTween.IsActive())
        {
            articFoxAttackTween.Kill();
            articFoxAttackTween = null;
        }

        transform.position = articFoxAttackOriginPosition;

        isArticFoxAttackMoving = false;
    }
    
    #endregion

    #region TakeDamage & DamageState
    /// <summary>
    /// 피격 시 데미지를 받음
    /// <para>1. 피가 절반 이하로 내려가면 Damage 상태로 전환</para>
    /// <para>2. 피가 0이하로 내려가면 Die 상태로 전환</para>
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (isDying || isDamaging) return;

        StartCoroutine(PlayHitEffect());

        float nextHp = currentHp - damage;
        bool triggerHalf = !halfHpTriggered && nextHp <= maxHp * 0.5f;

        if (triggerHalf)
        {
            pendingDamage = damage;
            halfHpTriggered = true;
            ChangeState(UnitState.Damage);
        }

        else
        {
            currentHp -= damage;

            if (damageAnimRoutine != null)
                StopCoroutine(damageAnimRoutine);

            damageAnimRoutine = StartCoroutine(DamageAnimationCoroutine());

            if (currentHp <= 0)
            {
                UTQ.RemoveUnit(team, this);
                ChangeState(UnitState.Die);
            }
        }
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
    }

    /// <summary>
    /// Damage State (피가 절반이하로 내려가면 1번만 실행)
    /// </summary>
    protected virtual void DamageState()
    {
        if (isDamaging) return;

        damageRoutine = StartCoroutine(DamageCoroutine());
    }

    protected virtual IEnumerator DamageCoroutine()
    {
        ResetArticFoxAttackPositionIfNeeded();

        isDamaging = true;

        StopAttack();

        damageTween?.Kill();

        bool removedFromList = false;
        float originalY = transform.position.y;
        Vector3 jumpEndPos = new Vector3(
            transform.position.x - direction * 0.8f,
            originalY,
            transform.position.z
        );

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(originalScale * 1.2f, 0.08f));
        seq.Join(spriteRenderer.DOFade(0.25f, 0.08f));

        seq.Append(
            transform.DOJump(jumpEndPos, 0.3f, 2, 0.6f)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() =>
                {
                    if (removedFromList || !HasMovedBehindFrontUnit()) return;

                    UTQ.Remove(this);
                    removedFromList = true;
                })
        );

        seq.Append(transform.DOScale(originalScale, 0.08f));
        seq.Join(spriteRenderer.DOFade(1f, 0.08f));

        seq.OnComplete(() =>
        {
            Vector3 pos = transform.position;
            pos.y = originalY;
            transform.position = pos;
        });

        damageTween = seq;

        yield return seq.WaitForCompletion();

        currentHp -= pendingDamage;

        if (currentHp <= 0)
        {
            currentHp = 0f;
            UTQ.RemoveUnit(team, this);
            ChangeState(UnitState.Die);
        }
        else
        {
            UTQ.Insert(this);
            ChangeState(UnitState.Attack);
        }

        isDamaging = false;
    }

    private bool HasMovedBehindFrontUnit()
    {
        return UTQ.HasMovedBehindAnotherUnit(this);
    }

    /// <summary>
    /// 피격 시 Dosclae & 반짝거리는 Animation
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 뭉게뭉게 구름 이펙트
    /// </summary>
    private IEnumerator PlayHitEffect()
    {
        if (hitEffectPrefab == null || isHitEffect) yield return null;

        isHitEffect = true;

        hitEffectPrefab.SetActive(true);

        float effectRange = 0.4f * unitSize;
        Vector3 randomPos = new Vector3(
            Random.Range(-effectRange/2, effectRange),
            Random.Range(0, effectRange),
            0
        );
        Vector3 spawnPos = transform.position + randomPos;
        hitEffectPrefab.transform.position = spawnPos;

        var effectRenderer = hitEffectPrefab.GetComponent<ParticleSystemRenderer>();
        if (effectRenderer != null)
        {
            effectRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            effectRenderer.sortingOrder = 30000 + Random.Range(0, 501);
        }

        var ps = hitEffectPrefab.GetComponent<ParticleSystem>();
        float duration = (ps != null) ? ps.main.duration : 1.0f;

        yield return new WaitForSeconds(duration);

        hitEffectPrefab.SetActive(false);
        isHitEffect = false;
    }

    /// <summary>
    /// 실행중이던 공격 중단
    /// </summary>
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

    //반대로 보고 달려가는 죽음 연출
    protected virtual IEnumerator DieCoroutine()
    {
        isDying = true;

        //큐에서 제거
        UTQ.RemoveUnit(team, this);

        //상태 설정
        StopAttack();
        isDamaging = false;

        damageTween?.Kill();
        transform.localScale = originalScale;
        spriteRenderer.color = Color.white;

        direction *= -1f;
        spriteRenderer.flipX = true;

        float elapsed = 0f;
        float duration = 2f;
        float runSpeed = moveSpeed * 2f;

        Color startColor = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            transform.position += Vector3.right * direction * runSpeed * Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        transform.localScale = originalScale;
        spriteRenderer.flipX = false;
        isDying = false;

        ownerPool.ReturnUnit(this);
    }
    #endregion

    #region Buff
    public void AddBuff(Buff buff)
    {
        _activeBuffs.Add(buff);
        UpdateFinalStats();
    }
    
    public void RemoveBuff(Buff buff)
    {
        if (_activeBuffs.Contains(buff))
        {
            _activeBuffs.Remove(buff);
            UpdateFinalStats();
        }
    }
    
    private void UpdateFinalStats()
    {
        RestoreStats();
        
        float moveSpeedMul = 1f;
        float attackSpeedMul = 1f;
        float attackDamageMul = 1f;

        foreach (Buff buff in _activeBuffs)
        {
            switch (buff.StatType)
            {
                case StatType.MoveSpeed:
                    moveSpeedMul += buff.Multiplier - 1;
                    break;
                case StatType.AttackSpeed:
                    attackSpeedMul += buff.Multiplier - 1;
                    break;
                case StatType.AttackDamage:
                    attackDamageMul += buff.Multiplier - 1;
                    break;
            }
        }
        
        moveSpeed = _originMoveSpeed * moveSpeedMul;
        attackSpeed = _originAttackSpeed * attackSpeedMul;
        attackDamage = _originAttackDamage * attackDamageMul;
    }

    private void RestoreStats()
    {
        moveSpeed = _originMoveSpeed;
        attackSpeed = _originAttackSpeed;
        attackDamage = _originAttackDamage;
    }
    #endregion
}
