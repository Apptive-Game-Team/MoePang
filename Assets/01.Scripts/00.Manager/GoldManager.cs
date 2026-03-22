using UnityEngine;

public class GoldManager : SingletonObject<GoldManager>
{
    [Header("소지금")]
    [SerializeField] private float gold;

    public float Gold => gold;

    public bool TrySpendGold(float amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;
        return true;
    }
}
