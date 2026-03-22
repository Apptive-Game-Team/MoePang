using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    public class SpecialPuzzleObject : PuzzleObject
    {
        public SpecialPuzzleType specialPuzzleType;
        public NormalPuzzleType colorBombType;

        public override int GetPuzzleSubType() => (int)specialPuzzleType;
        
        private void OnMouseUp()
        {
            if (Generator.IsProcessing)
            {
                return;
            }
            
            Generator.AddTask(() => Generator.ActivateSpecialBomb(column, row, specialPuzzleType));
        }
    }
}
