using UnityEngine;
using UnityEngine.Serialization;

namespace _01.Scripts._01.ThreeMatch
{
    public class SpecialPuzzleObject : PuzzleObject
    {
        public SpecialPuzzleType specialPuzzleType;
        public Habitat habitat;
        public bool isBlocked;

        public override int GetPuzzleSubType() => (int)specialPuzzleType;
        
        protected override void OnMouseUp()
        {
            base.OnMouseUp();
            if (Generator.IsProcessing)
            {
                return;
            }

            if (isBlocked)
            {
                isBlocked = false;
                return;
            }
            
            Generator.TryActivateSpecialPuzzle(column, row, specialPuzzleType);
        }
    }
}
