using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
    Transform GetTransform();
    float CurrentHp { get; }
    string GetName();
    TeamType GetTeam();
}
