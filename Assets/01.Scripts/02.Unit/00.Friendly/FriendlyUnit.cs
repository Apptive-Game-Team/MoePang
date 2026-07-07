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

    private int GetClearedHabitatStage(Habitat habitat)
    {
        return habitat switch
        {
            Habitat.Meadow => GameManager.Instance.playData.clearedMeadowHabitatStage,
            Habitat.Ocean => GameManager.Instance.playData.clearedOceanHabitatStage,
            Habitat.Desert => GameManager.Instance.playData.clearedDesertHabitatStage,
            Habitat.Forest => GameManager.Instance.playData.clearedForestHabitatStage,
            Habitat.Polar => GameManager.Instance.playData.clearedPolarHabitatStage,
            _ => 0
        };
    }
}
