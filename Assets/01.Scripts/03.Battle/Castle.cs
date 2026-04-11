using _01.Scripts._04.UI.InGame;
using UnityEngine;
using DG.Tweening;
using System.Collections;

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
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float shakeStrength = 0.05f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float flashAlpha = 0.4f;

    private static bool _isGameEnd;
    private GameObject _gameClearUI;
    private GameObject _gameOverUI;
    private GameObject EndUI => team == TeamType.Friendly ? _gameOverUI : _gameClearUI;
    
    private Tween damageTween;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPos;
    protected bool isHitEffect;

    //프로퍼티
    public float CurrentHp => currentHp;

    public Transform GetTransform() => transform;
    public string GetName() => name;
    public TeamType GetTeam() => team;

    private void Start()
    {
        if (team == TeamType.Friendly)
            maxHp = CastleManager.Instance.MaxHp;
        else maxHp = StageManager.Instance.CurrentStage + 1 * 100f;

        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPos = transform.localPosition;
        _isGameEnd = false;

        if (UnitTransformQueue.Instance != null)
        {
            UnitTransformQueue.Instance.RegisterCastle(team, this);
        }
        
        _gameClearUI = FindAnyObjectByType<GameClearUI>(FindObjectsInactive.Include).gameObject;
        _gameOverUI = FindAnyObjectByType<GameOverUI>(FindObjectsInactive.Include).gameObject;
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
        StartCoroutine(PlayHitEffect());

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private IEnumerator PlayHitEffect()
    {
        if (hitEffectPrefab == null || isHitEffect) yield return null;

        isHitEffect = true;

        hitEffectPrefab.SetActive(true);

        Vector3 randomPos = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0
        );
        Vector3 spawnPos = transform.position + randomPos;
        hitEffectPrefab.transform.position = spawnPos;

        var effectRenderer = hitEffectPrefab.GetComponent<ParticleSystemRenderer>();
        if (effectRenderer != null)
        {
            effectRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }

        var ps = hitEffectPrefab.GetComponent<ParticleSystem>();
        float duration = (ps != null) ? ps.main.duration : 1.0f;

        yield return new WaitForSeconds(duration);

        hitEffectPrefab.SetActive(false);
        isHitEffect = false;
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

        if (!_isGameEnd)
        {
            _isGameEnd = true;
            EndUI.SetActive(true);
        }
    }
}
