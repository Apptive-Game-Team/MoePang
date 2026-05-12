using System;
using UnityEngine;

/// <summary>
/// 유닛 단계에 따른 스탯 가중치 설정 매니저
/// </summary>
public class UnitGradeManager : MonoBehaviour
{
    public static UnitGradeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetFriendlyUnitGradeStat(Unit unit)
    {
        switch (unit.Data.UnitGrade)
        {
            case 2:
                unit.AttackSpeedMultiplier(2f);
                break;
            
            case 3:
                unit.AttackRangeMultiplier(2f);
                break;
            
            case 4:
                unit.MoveSpeedMultiplier(0.5f);
                unit.UnitHpMultiplier(3f);
                unit.AttackRangeMultiplier(0.7f);
                break;
            
            default:
                break;
        }
    }
}
