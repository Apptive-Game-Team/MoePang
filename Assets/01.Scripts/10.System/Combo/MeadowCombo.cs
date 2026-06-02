using _01.Scripts._00.Manager;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _01.Scripts._10.System.Combo
{
    [CreateAssetMenu(menuName = "Combo/MeadowCombo", fileName = "MeadowCombo")]
    public class MeadowCombo : Combo
    {
        public override void TriggerComboEffect(ComboContext context)
        {
            var types = Enum.GetValues(typeof(Habitat));
            
            for (int i = 0; i < GameManager.Instance.comboData.comboLevels[info.comboType]; i++)
            {
                context.UnitSpawner.SpawnHighestFriendly((Habitat)types.GetValue(Random.Range(0, types.Length)));
            }
        }

        public override string DynamicDescription()
        {
            int level = GameManager.Instance.comboData.comboLevels[info.comboType];
            
            if (level == info.ComboMaxLevel)
            {
                return string.Format(info.comboDescription, level, "");   
            }
            return string.Format(info.comboDescription, level, $"/<color=grey>{1 + level}</color>");
        }
    }
}
