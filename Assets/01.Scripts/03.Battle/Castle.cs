using _01.Scripts._04.UI.InGame;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 각 팀의 성 관리 스크립트
/// </summary>
public class Castle : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private TeamType team;
    [SerializeField] protected float maxHp;
    [SerializeField] protected float currentHp;

    [Header("피격 연출")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private int maxHitEffectCount = 5;
    [SerializeField] private float shakeStrength = 0.05f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float flashAlpha = 0.4f;

    //내부 컴포넌트
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPos;

    //상태 관리

    //로직 제어
    private Tween damageTween;
    private readonly List<GameObject> activeHitEffects = new List<GameObject>();
    
    public float CurrentHp => currentHp;
    public Transform GetTransform() => transform;
    public string GetName() => name;
    public TeamType GetTeam() => team;

    private void Start()
    {
        if (team == TeamType.Friendly)
        {
            maxHp = CastleManager.Instance.MaxHp;
        }
        else
        {
            int currentStage = StageManager.Instance.CurrentStage + 1;
            maxHp = BalanceFormula.GetEnemyCastleMaxHp(currentStage);
        }

        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPos = transform.localPosition;

        if (UnitTransformQueue.Instance != null)
        {
            UnitTransformQueue.Instance.RegisterCastle(team, this);
        }
    }

    #region 피격 & 피격 연출
    public void TakeDamage(float damage, Sprite hitSprite = null)
    {
        currentHp -= damage;
        StartCoroutine(PlayHitEffect(hitSprite));

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

    private IEnumerator PlayHitEffect(Sprite hitSprite = null)
    {
        if (hitEffectPrefab == null)
            yield break;

        activeHitEffects.RemoveAll(effect => effect == null);
        if (activeHitEffects.Count >= maxHitEffectCount)
            yield break;

        Vector3 randomPos = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.3f, 0.3f),
            0
        );
        Vector3 spawnPos = transform.position + randomPos;
        GameObject hitEffect = Instantiate(
            hitEffectPrefab,
            spawnPos,
            hitEffectPrefab.transform.rotation,
            hitEffectPrefab.transform.parent
        );
        activeHitEffects.Add(hitEffect);
        hitEffect.SetActive(true);

        var particleRenderer = hitEffect.GetComponent<ParticleSystemRenderer>();
        var particleSystem = hitEffect.GetComponent<ParticleSystem>();
        var hitSpriteRenderer = hitEffect.GetComponent<SpriteRenderer>();

        if (hitSprite != null)
        {
            if (hitSpriteRenderer == null)
                hitSpriteRenderer = hitEffect.AddComponent<SpriteRenderer>();

            if (particleRenderer != null)
                particleRenderer.enabled = false;

            if (particleSystem != null)
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            hitSpriteRenderer.enabled = true;
            hitSpriteRenderer.sprite = hitSprite;
            hitSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            hitSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }
        else if (particleRenderer != null)
        {
            particleRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }

        float duration = (particleSystem != null) ? particleSystem.main.duration : 1.0f;

        yield return new WaitForSeconds(duration);

        activeHitEffects.Remove(hitEffect);
        Destroy(hitEffect);
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
    #endregion

    private void Die()
    {
        Debug.Log("타워가 아파요 ㅠ");

        if (team == TeamType.Friendly)
        {
            StageManager.Instance.GameOver();
        }
        else
        {
            StageManager.Instance.GameClear();
        }
    }
}
