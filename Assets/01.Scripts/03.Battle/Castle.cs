using UnityEngine;
using DG.Tweening;

/// <summary>
/// 각 팀의 성
/// </summary>
public class Castle : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private TeamType team;
    [SerializeField] protected float maxHp;
    [SerializeField] protected float currentHp;

    [Header("피격 연출")]
    [SerializeField] private float shakeStrength = 0.05f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float flashAlpha = 0.4f;

    private Tween damageTween;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPos;

    //프로퍼티
    public float CurrentHp => currentHp;

    public Transform GetTransform() => transform;
    public string GetName() => name;
    public TeamType GetTeam() => team;

    private void Start()
    {
        maxHp = CastleManager.Instance.MaxHp;
        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPos = transform.localPosition;

        if (UnitTransformQueue.Instance != null)
        {
            UnitTransformQueue.Instance.RegisterCastle(team, this);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;

        if (damageTween != null && damageTween.IsActive() && damageTween.IsPlaying())
        {
            if (currentHp <= 0)
                Die();

            return;
        }

        PlayDamageEffect();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void PlayDamageEffect()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(spriteRenderer.DOFade(flashAlpha, 0.06f));
        seq.Join(transform.DOLocalMoveX(originalPos.x + shakeStrength, 0.05f));

        seq.Append(spriteRenderer.DOFade(1f, 0.06f));
        seq.Join(transform.DOLocalMoveX(originalPos.x - shakeStrength, 0.05f));

        seq.Append(transform.DOLocalMoveX(originalPos.x, 0.05f));

        damageTween = seq;
    }

    private void Die()
    {
        Debug.Log("타워가 아파요 ㅠ");
    }
}
