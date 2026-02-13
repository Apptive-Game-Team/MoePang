using UnityEngine;
using UnityEngine.Serialization;

namespace ThreeMatch
{
    public class ObstaclePuzzleObject : PuzzleObject
    {
        public ObstaclePuzzleType obstaclePuzzleType;
        public NormalPuzzleType normalPuzzleType;
        
        public override int GetPuzzleSubType() => (int)obstaclePuzzleType;
    }
}
