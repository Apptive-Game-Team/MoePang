using _01.Scripts._00.Manager;
using UnityEngine;

namespace _01.Scripts._10.System.Combo
{
    [CreateAssetMenu(menuName = "Combo/OceanCombo", fileName = "OceanCombo")]
    public class OceanCombo : Combo
    {
        public override void TriggerComboEffect(ComboContext context)
        {
            Debug.Log("Ocean Combo Applied");
            
            float multiplier = 1.2f + GameManager.Instance.comboData.ComboLevels[info.comboType] * 0.04f;
            float duration = 5f + GameManager.Instance.comboData.ComboLevels[info.comboType] * 0.25f;
            
            BuffManager.Instance.ApplyAllyBuff(StatType.AttackDamage, multiplier, duration);
            BuffManager.Instance.ApplyAllyBuff(StatType.AttackSpeed, multiplier, duration);
            BuffManager.Instance.ApplyAllyBuff(StatType.MoveSpeed, multiplier, duration);
        }

        public override string DynamicDescription()
        {
            int level = GameManager.Instance.comboData.ComboLevels[info.comboType];
            float mul1 = 1 + level * 0.2f;
            float mul2 = 1 + (level + 1) * 0.2f;
            
            if (level == info.ComboMaxLevel)
            {
                return string.Format(info.comboDescription, mul1, "");   
            }
            return string.Format(info.comboDescription, mul1, $"/<color=grey>{mul2}</color>");
        }
    }
}
