using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 유닛이 싸우는 무기
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("무기 스텟")]
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float direction = 1f;
    [SerializeField] protected LayerMask targetLayer;

    private void Update()
    {
        transform.position += Vector3.right * moveSpeed * direction * Time.deltaTime;
    }

    public void SetWeapon(float dir, LayerMask layermask)
    {
        direction = dir;
        targetLayer = layermask;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
            return;

        Unit unit = collision.gameObject.GetComponent<Unit>();
        Castle castle = collision.gameObject.GetComponent<Castle>();
        if (unit != null)
        {
            unit.TakeDamage(attackDamage);
        }

        if (castle  != null)
        {
            castle.TakeDamage (attackDamage);
        }

        Destroy(gameObject);
    }
}
