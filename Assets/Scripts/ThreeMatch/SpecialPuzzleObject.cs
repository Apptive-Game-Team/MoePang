using UnityEngine;

namespace ThreeMatch
{
    public class SpecialPuzzleObject : PuzzleObject
    {
        public SpecialPuzzleType specialPuzzleType;
        public NormalPuzzleType colorBombType;

        public override int GetPuzzleSubType() => (int)specialPuzzleType;
    }
}
