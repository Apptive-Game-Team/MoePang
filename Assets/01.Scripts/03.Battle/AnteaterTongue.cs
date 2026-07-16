using UnityEngine;

public class AnteaterTongue : RangeAttackPrefab
{
    [Header("Tongue")]
    [SerializeField] private Transform tongueVisual;
    [SerializeField] private BoxCollider2D tongueCollider;
    [SerializeField] private float maxLength = 3f;
    [SerializeField] private float extendSpeed = 12f;
    [SerializeField] private float retractSpeed = 18f;
    [SerializeField] private float tongueWidth = 0.25f;
    [SerializeField] private float spriteUnitLength = 1f;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0.3f, -0.15f);

    private Vector3 originalVisualScale;
    private float currentLength;
    private bool retracting;
    private bool damaged;

    protected override void Awake()
    {
        base.Awake();

        if (tongueCollider == null)
        {
            tongueCollider = GetComponent<BoxCollider2D>();
        }

        if (tongueVisual == null)
        {
            Debug.LogError($"{name}: Tongue Visual이 비어있습니다. 자식 SpriteRenderer Transform을 넣어주세요.");
            enabled = false;
            return;
        }

        originalVisualScale = tongueVisual.localScale;
    }

    public override void Init(float damage, TeamType ownerTeam, LayerMask targetLayer, Vector2 direction)
    {
        this.damage = damage;
        this.ownerTeam = ownerTeam;
        this.targetLayer = targetLayer;
        this.direction = direction.normalized;
        initialized = true;

        float sign = this.direction.x >= 0f ? 1f : -1f;
        transform.position += new Vector3(spawnOffset.x * sign, spawnOffset.y, 0f);
        
        currentLength = 0f;
        retracting = false;
        damaged = false;

        SetLength(0f);

        Destroy(gameObject, lifeTime);
    }

    protected override void Update()
    {
        if (!initialized) return;

        if (retracting)
        {
            Retract();
        }
        else
        {
            Extend();
        }
    }

    private void Extend()
    {
        currentLength += extendSpeed * Time.deltaTime;
        currentLength = Mathf.Min(currentLength, maxLength);

        SetLength(currentLength);

        if (currentLength >= maxLength)
        {
            retracting = true;
        }
    }

    private void Retract()
    {
        currentLength -= retractSpeed * Time.deltaTime;
        currentLength = Mathf.Max(currentLength, 0f);

        SetLength(currentLength);

        if (currentLength <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void SetLength(float length)
    {
        tongueVisual.localPosition = new Vector3(length * 0.5f, 0f, 0f);

        Vector3 scale = originalVisualScale;
        scale.x = length / spriteUnitLength;
        tongueVisual.localScale = scale;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (damaged) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;
        if (target.GetTeam() == ownerTeam) return;

        target.TakeDamage(damage);
        damaged = true;
        Destroy(gameObject);
        //retracting = true;
    }
}