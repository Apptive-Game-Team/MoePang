using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Sprite hitSprite = null);
    Transform GetTransform();
    float CurrentHp { get; }
    string GetName();
    TeamType GetTeam();
}
