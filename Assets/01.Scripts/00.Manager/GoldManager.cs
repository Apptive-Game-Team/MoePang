using _01.Scripts._00.Manager;
using System;
using UnityEngine;

public class GoldManager : SingletonObject<GoldManager>
{
    [Header("소지금")]
    [SerializeField] private int gold;
    [SerializeField] private int dia;

    public int Gold => gold;
    public int Dia => dia;
    public event Action OnGoldChanged;
    public event Action OnDiaChanged;

    protected override void Awake()
    {
        base.Awake();
        
        gold = GameManager.Instance.playData.goldAmount;
    }

    public bool TrySpendGold(int amount)
    {
        return gold >= amount;
    }

    public bool TrySpendDia(int amount)
    {
        return dia >= amount;
    }

    public void AdjustGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke();
    }

    public void AdjustDia(int amount)
    {
        dia += amount;
        OnDiaChanged?.Invoke();
    }

    public void AddStageClearedGold()
    {
        int amount = StageManager.Instance.CheckClearedStage() ? 20 : 100;
        AdjustGold(amount);
    }
}
