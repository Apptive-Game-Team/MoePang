using _01.Scripts._00.Manager;
using _01.Scripts._04.UI.InGame;
using System;
using UnityEngine;
using _01.Scripts._11.HabitatMode;

public class StageManager : SingletonObject<StageManager>
{
    public int MaxStage { get; private set; }
    public int CurrentStage { get; private set; }
    public float CurrentTime { get; private set; }
    public int UsedTileCount { get; private set; }
    public bool IsTimerRunning { get; private set; }
    
    public int MaxMeadowHabitatStage { get; private set; }
    public int MaxOceanHabitatStage { get; private set; }
    public int MaxDesertHabitatStage { get; private set; }
    public int MaxForestHabitatStage { get; private set; }
    public int MaxPolarHabitatStage { get; private set; }
    
    public int CurrentMeadowHabitatStage { get; private set; }
    public int CurrentOceanHabitatStage { get; private set; }
    public int CurrentDesertHabitatStage { get; private set; }
    public int CurrentForestHabitatStage { get; private set; }
    public int CurrentPolarHabitatStage { get; private set; }

    public int CurrentHabitatStage => GetHabitatStage(HabitatModeManager.Instance.HabitatMode);
    
    public int DifficultyStage
    {
        get
        {
            if (HabitatModeManager.Instance != null &&
                HabitatModeManager.Instance.IsHabitatBattle)
            {
                return CurrentHabitatStage + 50;
            }

            return CurrentStage;
        }
    }
    
    public event Action<float> OnTimeChanged;

    protected override void Awake()
    {
        base.Awake();

        PlayData playData = GameManager.Instance.playData;
        MaxStage = playData.MaxStages[StageType.Normal];
        MaxMeadowHabitatStage = playData.MaxStages[StageType.Meadow];
        MaxOceanHabitatStage = playData.MaxStages[StageType.Ocean];
        MaxDesertHabitatStage = playData.MaxStages[StageType.Desert];
        MaxForestHabitatStage = playData.MaxStages[StageType.Forest];
        MaxPolarHabitatStage = playData.MaxStages[StageType.Polar];

        CurrentStage = playData.selectedStage[(int)StageType.Normal];
        CurrentMeadowHabitatStage = playData.selectedStage[(int)StageType.Meadow];
        CurrentOceanHabitatStage = playData.selectedStage[(int)StageType.Ocean];
        CurrentDesertHabitatStage = playData.selectedStage[(int)StageType.Desert];
        CurrentForestHabitatStage = playData.selectedStage[(int)StageType.Forest];
        CurrentPolarHabitatStage = playData.selectedStage[(int)StageType.Polar];
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

    public void GameClear()
    {
        FindAnyObjectByType<GameClearUI>(FindObjectsInactive.Include).gameObject.SetActive(true);
    }

    public void GameOver()
    {
        FindAnyObjectByType<GameOverUI>(FindObjectsInactive.Include).gameObject.SetActive(true);
    }
    
    public int GetHabitatStage(HabitatMode mode)
    {
        return mode switch
        {
            HabitatMode.MeadowMode => CurrentMeadowHabitatStage,
            HabitatMode.OceanMode => CurrentOceanHabitatStage,
            HabitatMode.DesertMode => CurrentDesertHabitatStage,
            HabitatMode.ForestMode => CurrentForestHabitatStage,
            HabitatMode.PolarMode => CurrentPolarHabitatStage,
            _ => CurrentMeadowHabitatStage
        };
    }
    
    public int GetMaxHabitatStage(HabitatMode mode)
    {
        return mode switch
        {
            HabitatMode.MeadowMode => MaxMeadowHabitatStage,
            HabitatMode.OceanMode => MaxOceanHabitatStage,
            HabitatMode.DesertMode => MaxDesertHabitatStage,
            HabitatMode.ForestMode => MaxForestHabitatStage,
            HabitatMode.PolarMode => MaxPolarHabitatStage,
            _ => MaxMeadowHabitatStage
        };
    }
    
    public void SetHabitatStage(HabitatMode mode, int stage)
    {
        stage = Mathf.Clamp(stage, 0, GetMaxHabitatStage(mode));

        switch (mode)
        {
            case HabitatMode.MeadowMode:
                CurrentMeadowHabitatStage = stage;
                break;
            case HabitatMode.OceanMode:
                CurrentOceanHabitatStage = stage;
                break;
            case HabitatMode.DesertMode:
                CurrentDesertHabitatStage = stage;
                break;
            case HabitatMode.ForestMode:
                CurrentForestHabitatStage = stage;
                break;
            case HabitatMode.PolarMode:
                CurrentPolarHabitatStage = stage;
                break;
        }
    }

    public void AddHabitatStage(HabitatMode mode, int num)
    {
        SetHabitatStage(mode, GetHabitatStage(mode) + num);
    }

    public void SetMaxHabitatStage(HabitatMode mode, int stage)
    {
        stage = Mathf.Max(0, stage);

        switch (mode)
        {
            case HabitatMode.MeadowMode:
                MaxMeadowHabitatStage = stage;
                break;
            case HabitatMode.OceanMode:
                MaxOceanHabitatStage = stage;
                break;
            case HabitatMode.DesertMode:
                MaxDesertHabitatStage = stage;
                break;
            case HabitatMode.ForestMode:
                MaxForestHabitatStage = stage;
                break;
            case HabitatMode.PolarMode:
                MaxPolarHabitatStage = stage;
                break;
        }
    }
    
    public void RefreshFromPlayData()
    {
        PlayData playData = GameManager.Instance.playData;
        
        CurrentStage = playData.selectedStage[(int)StageType.Normal];
        CurrentMeadowHabitatStage = playData.selectedStage[(int)StageType.Meadow];
        CurrentOceanHabitatStage = playData.selectedStage[(int)StageType.Ocean];
        CurrentDesertHabitatStage = playData.selectedStage[(int)StageType.Desert];
        CurrentForestHabitatStage = playData.selectedStage[(int)StageType.Forest];
        CurrentPolarHabitatStage = playData.selectedStage[(int)StageType.Polar];
    }
}
