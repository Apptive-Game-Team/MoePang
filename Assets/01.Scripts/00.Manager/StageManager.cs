using _01.Scripts._00.Manager;
using System;
using UnityEngine;

public class StageManager : SingletonObject<StageManager>
{
    public int MaxStage { get; private set; }
    public int CurrentStage { get; private set; }
    public float CurrentTime { get; private set; }
    public int UsedTileCount { get; private set; }
    public bool IsTimerRunning { get; private set; }
    
    public event Action<float> OnTimeChanged;

    protected override void Awake()
    {
        base.Awake();
        
        MaxStage = GameManager.Instance.playData.clearedStage + 1;
        CurrentStage = MaxStage;
    }
    
    private void Update()
    {
        if (!IsTimerRunning) return;

        CurrentTime += Time.deltaTime;
        
        OnTimeChanged?.Invoke(CurrentTime);
    }

    public void SetMaxStage(int maxStage)
    {
        MaxStage = maxStage;
    }

    public void SetStage(int currentStage)
    {
        CurrentStage = currentStage;
    }

    public void AddStage(int num)
    {
        CurrentStage += num;
    }

    public void SetUsedTile(int num)
    {
        UsedTileCount += num;
    }

    public void StartStage()
    {
        CurrentTime = 0f;
        UsedTileCount = 0;
        IsTimerRunning = true;
    }

    public void StopStage()
    {
        IsTimerRunning = false;
    }

    public bool CheckClearedStage()
    {
        return CurrentStage < MaxStage;
    }
}
