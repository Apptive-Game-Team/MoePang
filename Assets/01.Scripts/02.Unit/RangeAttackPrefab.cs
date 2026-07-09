using UnityEngine;

public class RangeAttackPrefab : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float lifeTime = 3f;

    private float damage;
    private TeamType ownerTeam;
    private LayerMask targetLayer;
    private Vector2 direction;
    private bool initialized;
    
    public void Init(float damage, TeamType ownerTeam, LayerMask targetLayer, Vector2 direction)
    {
        this.damage = damage;
        this.ownerTeam = ownerTeam;
        this.targetLayer = targetLayer;
        this.direction = direction.normalized;
        initialized = true;

        Destroy(gameObject, lifeTime);
    }
    
    private void Update()
    {
        if (!initialized) return;

        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;
        if (target.GetTeam() == ownerTeam) return;

        target.TakeDamage(damage);
        Destroy(gameObject);
    }
}
