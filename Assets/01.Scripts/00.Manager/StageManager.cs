using System;
using UnityEngine;

public class StageManager : SingletonObject<StageManager>
{
    private int _currentStage;
    public int CurrentStage => _currentStage;
    
    public float CurrentTime { get; private set; }
    public bool IsTimerRunning { get; private set; }
    
    public event Action<float> OnTimeChanged;

    private void Start()
    {
        StartStage();
    }
    
    private void Update()
    {
        if (!IsTimerRunning) return;

        CurrentTime += Time.deltaTime;
        
        OnTimeChanged?.Invoke(CurrentTime);
    }

    public void SetStage(int num)
    {
        _currentStage += num;
    }

    public void StartStage()
    {
        CurrentTime = 0f;
        IsTimerRunning = true;
    }

    public void StopStage()
    {
        IsTimerRunning = false;
    }
}
