using UnityEngine;

/// <summary>
/// 원거리 공격 Attack Prefab 스크립트
/// </summary>
public class RangeAttackPrefab : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float lifeTime = 3f;

    [Header("Animation")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Sprite[] animationFrames;
    [SerializeField] protected float framesPerSecond = 12f;

    [Header("Hit Effect")]
    [SerializeField] protected Sprite hitSprite;

    protected float damage;
    protected TeamType ownerTeam;
    protected LayerMask targetLayer;
    protected Vector2 direction;
    protected bool initialized;

    private int currentFrameIndex;
    private float animationTimer;

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && animationFrames != null && animationFrames.Length > 0)
        {
            spriteRenderer.sprite = animationFrames[0];
        }
    }

    public virtual void Init(float damage, TeamType ownerTeam, LayerMask targetLayer, Vector2 direction)
    {
        this.damage = damage;
        this.ownerTeam = ownerTeam;
        this.targetLayer = targetLayer;
        this.direction = direction.normalized;
        initialized = true;

        Destroy(gameObject, lifeTime);
    }

    protected virtual void Update()
    {
        PlayAnimation();

        if (!initialized) return;

        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    private void PlayAnimation()
    {
        if (spriteRenderer == null) return;
        if (animationFrames == null || animationFrames.Length == 0) return;
        if (framesPerSecond <= 0f) return;

        if (currentFrameIndex >= animationFrames.Length - 1)
        {
            spriteRenderer.sprite = animationFrames[animationFrames.Length - 1];
            return;
        }

        animationTimer += Time.deltaTime;

        float frameDuration = 1f / framesPerSecond;
        if (animationTimer < frameDuration) return;

        animationTimer -= frameDuration;
        currentFrameIndex++;
        spriteRenderer.sprite = animationFrames[currentFrameIndex];
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;
        if (target.GetTeam() == ownerTeam) return;

        target.TakeDamage(damage, hitSprite);
        Destroy(gameObject);
    }
}
