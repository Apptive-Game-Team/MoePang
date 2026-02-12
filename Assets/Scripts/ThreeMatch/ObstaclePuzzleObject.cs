using UnityEngine;

namespace ThreeMatch
{
    public class ObstaclePuzzleObject : PuzzleObject
    {
        public ObstaclePuzzleType obstaclePuzzleType;
        
        public override int GetPuzzleSubType() => (int)obstaclePuzzleType;
    }
}
