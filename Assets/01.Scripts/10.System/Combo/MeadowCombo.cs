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
            Debug.Log("Meadow Combo Applied");
            
            var types = Enum.GetValues(typeof(Habitat));
            int spawnCount = 1 + GameManager.Instance.comboData.ComboLevels[info.comboType] / 2;
            
            for (int i = 0; i < spawnCount; i++)
            {
                context.UnitSpawner.SpawnHighestFriendly((Habitat)types.GetValue(Random.Range(0, types.Length)));
            }
        }

        public override string DynamicDescription()
        {
            int level = 1 + GameManager.Instance.comboData.ComboLevels[info.comboType] / 2;
            int nextLevel = 1 + (1 + GameManager.Instance.comboData.ComboLevels[info.comboType]) / 2;
            
            if (level == info.ComboMaxLevel)
            {
                return string.Format(info.comboDescription, level, "");   
            }
            return string.Format(info.comboDescription, level, $"/<color=grey>{nextLevel}</color>");
        }
    }
}
