using _01.Scripts._00.Manager;
using System;
using UnityEngine;
using _01.Scripts._11.HabitatMode;

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
        dia = GameManager.Instance.playData.diaAmount;
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
        int stage;
        bool alreadyCleared;

        if (HabitatModeManager.Instance != null &&
            HabitatModeManager.Instance.IsHabitatBattle)
        {
            HabitatMode mode = HabitatModeManager.Instance.HabitatMode;

            stage = StageManager.Instance.GetHabitatStage(mode) + 1 + 50;

            alreadyCleared =
                StageManager.Instance.GetHabitatStage(mode) <
                StageManager.Instance.GetMaxHabitatStage(mode);
        }
        else
        {
            stage = StageManager.Instance.CurrentStage + 1;
            alreadyCleared = StageManager.Instance.CheckClearedStage();
        }
        
        float amount = alreadyCleared
            ? 20f + 4f * Mathf.Sqrt(stage - 1)
            : 100f + 20f * (Mathf.Sqrt(stage) - 1f);

        AdjustGold(Mathf.CeilToInt(amount));
    }
    
    public int AddStageClearedDia()
    {
        if (HabitatModeManager.Instance == null ||
            !HabitatModeManager.Instance.IsHabitatBattle)
        {
            return 0;
        }

        HabitatMode mode = HabitatModeManager.Instance.HabitatMode;
        int currentStage = StageManager.Instance.GetHabitatStage(mode);
        int stage = currentStage + 1;

        bool alreadyCleared =
            currentStage < StageManager.Instance.GetMaxHabitatStage(mode);

        float amount = alreadyCleared
            ? 5f + stage
            : 10f + 3f * stage;

        if (HabitatModeManager.Instance.IsHabitatModeEventDay(mode))
        {
            amount *= 1.5f;
        }

        int diaAmount = Mathf.RoundToInt(amount);
        AdjustDia(diaAmount);

        return diaAmount;
    }
}
