using _01.Scripts._00.Manager;
using UnityEngine;

namespace _01.Scripts._10.System.Combo
{
    [CreateAssetMenu(menuName = "Combo/OceanCombo", fileName = "OceanCombo")]
    public class OceanCombo : Combo
    {
        public override void TriggerComboEffect(ComboContext context)
        {
            float multiplier = 1 + GameManager.Instance.comboData.comboLevels[info.comboType] * 0.2f;
            
            BuffManager.Instance.ApplyAllyBuff(StatType.AttackDamage, multiplier, 5f);
            BuffManager.Instance.ApplyAllyBuff(StatType.AttackSpeed, multiplier, 5f);
            BuffManager.Instance.ApplyAllyBuff(StatType.MoveSpeed, multiplier, 5f);
        }

        public override string DynamicDescription()
        {
            int level = GameManager.Instance.comboData.comboLevels[info.comboType];
            float mul1 = 1 + level * 0.2f;
            float mul2 = 1 + (level + 1) * 0.2f;
            
            if (level == info.ComboMaxLevel)
            {
                return string.Format(info.comboDescription, mul1, "");   
            }
            return string.Format(info.comboDescription, mul1, $"/<color=gray>{mul2}</color>");
        }
    }
}
