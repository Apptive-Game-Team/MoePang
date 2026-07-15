

using _01.Scripts._00.Manager;
using UnityEngine;

namespace _01.Scripts._10.System.Combo
{
    [CreateAssetMenu(menuName = "Combo/ForestCombo", fileName = "ForestCombo")]
    public class ForestCombo : Combo
    {
        public override void TriggerComboEffect(ComboContext context)
        {
            int level = GameManager.Instance.comboData.ComboLevels[info.comboType];
            float mul = 0.1f + level * 0.02f;
            
            FriendlyUnit[] allies = FindObjectsByType<FriendlyUnit>(FindObjectsSortMode.None);
            foreach (FriendlyUnit ally in allies)
            {
                ally.Heal(ally.MaxHp * mul);
            }
        }

        public override string DynamicDescription()
        {
            int level = GameManager.Instance.comboData.ComboLevels[info.comboType];
            int mul1 = 10 + level * 2;
            int mul2 = 10 + (level + 1) * 2;
            
            if (level == info.ComboMaxLevel)
            {
                return string.Format(info.comboDescription, $"{mul1}%", "");   
            }
            return string.Format(info.comboDescription, $"{mul1}%", $"/<color=grey>{mul2}%</color>");
        }
    }
}
