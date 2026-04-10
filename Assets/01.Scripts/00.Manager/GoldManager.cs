using _01.Scripts._00.Manager;
using System;
using UnityEngine;

public class GoldManager : SingletonObject<GoldManager>
{
    [Header("소지금")]
    [SerializeField] private int gold;

    public int Gold => gold;
    public event Action OnGoldChanged;

    protected override void Awake()
    {
        base.Awake();
        
        gold = GameManager.Instance.playData.goldAmount;
    }

    public bool TrySpendGold(int amount)
    {
        if (gold < amount)
        {
            return false;
        }

        gold -= amount;
        OnGoldChanged?.Invoke();
        return true;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke();
    }

    public void AddStageClearedGold()
    {
        int amount = StageManager.Instance.CheckClearedStage() ? 20 : 100;
        AddGold(amount);
    }
}
