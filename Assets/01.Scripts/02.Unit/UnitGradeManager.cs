using UnityEngine;

/// <summary>
/// 유닛 단계에 따른 스탯 가중치 설정 매니저
/// </summary>
public class UnitGradeManager : SingletonObject<UnitGradeManager>
{
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
        }
    }
}
