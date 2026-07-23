using _01.Scripts._00.Manager;
using UnityEngine;
using _01.Scripts._11.HabitatMode;

public class FriendlyUnit : Unit
{
    [Header("아군 유닛 전용 데이터")]
    [SerializeField] private Habitat habitat;

    private FriendlyUnitData friendlyData;
    
    public override void SetData(UnitData data)
    {
        base.SetData(data);

        friendlyData = data as FriendlyUnitData;

        direction = 1f;

        habitat = friendlyData.Habitat;

        ApplyHabitatClearBonus();

        targetLayer = LayerMask.GetMask("Enemy");
    }
    
    protected override void SetProceedStat()
    {
        base.SetProceedStat();
    }
    
    private void ApplyHabitatClearBonus()
    {
        if (GameManager.Instance == null || GameManager.Instance.playData == null)
        {
            return;
        }

        int clearedStage = GetClearedHabitatStage(habitat);

        if (clearedStage <= 0)
        {
            return;
        }

        float bonusMaxHp = clearedStage * 10f;
        float bonusAttackDamage = clearedStage * 1f;

        ApplyBaseHpAndAttackDamage(
            maxHp + bonusMaxHp,
            attackDamage + bonusAttackDamage
        );
    }

    private int GetClearedHabitatStage(Habitat type)
    {
        return type switch
        {
            Habitat.Meadow => GameManager.Instance.playData.MaxStages[StageType.Meadow],
            Habitat.Ocean => GameManager.Instance.playData.MaxStages[StageType.Ocean],
            Habitat.Desert => GameManager.Instance.playData.MaxStages[StageType.Desert],
            Habitat.Forest => GameManager.Instance.playData.MaxStages[StageType.Forest],
            Habitat.Polar => GameManager.Instance.playData.MaxStages[StageType.Polar],
            _ => 0
        };
    }
}
