using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using UnityEngine;

namespace _01.Scripts._10.System.Combo
{
    [CreateAssetMenu(menuName = "Combo/DesertCombo", fileName = "DesertCombo")]
    public class DesertCombo : Combo
    {
        public override void TriggerComboEffect(ComboContext context)
        {
            for (int i = 0; i < 1 + ((GameManager.Instance.comboData.comboLevels[info.comboType] - 1) / 2); i++)
            {
                context.Puzzle.SpawnSpecialPuzzle(SpecialPuzzleType.RowBomb);
            }
        }

        public override string DynamicDescription()
        {
            int level = GameManager.Instance.comboData.comboLevels[info.comboType];
            
            if (level == info.ComboMaxLevel)
            {
                return string.Format(info.comboDescription, 1 + ((level - 1) / 2), "");   
            }
            return string.Format(info.comboDescription, 1 + ((level - 1) / 2), $"/<color=gray>{1 + (level / 2)}</color>");
        }
    }
}
