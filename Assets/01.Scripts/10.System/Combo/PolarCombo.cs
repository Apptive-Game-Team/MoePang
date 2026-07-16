using _01.Scripts._00.Manager;
using UnityEngine;

namespace _01.Scripts._10.System.Combo
{
    [CreateAssetMenu(menuName = "Combo/PolarCombo", fileName = "PolarCombo")]
    public class PolarCombo : Combo
    {
        public override void TriggerComboEffect(ComboContext context)
        {
            if (GameManager.Instance.playData.MaxStages[StageType.Polar] < 5)
            {
                return;
            }
            
            Debug.Log("Polar Combo Applied");
            
            float multiplier = 0.9f - GameManager.Instance.comboData.ComboLevels[info.comboType] * 0.025f;
            float duration = 5f + GameManager.Instance.comboData.ComboLevels[info.comboType] * 0.25f;
            
            BuffManager.Instance.ApplyEnemyBuff(StatType.AttackSpeed, multiplier, duration);
            BuffManager.Instance.ApplyEnemyBuff(StatType.MoveSpeed, multiplier, duration);
        }

        public override string DynamicDescription()
        {
            int level = GameManager.Instance.comboData.ComboLevels[info.comboType];
            float mul1 = 1 - level * 0.1f;
            float mul2 = 1 - (level + 1) * 0.1f;
            
            if (level == info.ComboMaxLevel)
            {
                return string.Format(info.comboDescription, mul1, "");   
            }
            return string.Format(info.comboDescription, mul1, $"/<color=grey>{mul2}</color>");
        }
    }
}
