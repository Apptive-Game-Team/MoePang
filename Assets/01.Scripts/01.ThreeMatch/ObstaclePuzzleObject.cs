namespace _01.Scripts._01.ThreeMatch
{
    public class ObstaclePuzzleObject : PuzzleObject
    {
        public ObstaclePuzzleType obstaclePuzzleType;
        public NormalPuzzleType normalPuzzleType;

        public bool isTriggered;
        
        public override int GetPuzzleSubType() => (int)obstaclePuzzleType;
    }
}
