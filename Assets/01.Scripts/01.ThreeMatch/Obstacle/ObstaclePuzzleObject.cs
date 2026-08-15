namespace _01.Scripts._01.ThreeMatch.Obstacle
{
    public class ObstaclePuzzleObject : PuzzleObject
    {
        public ObstaclePuzzleType obstaclePuzzleType;
        public Habitat habitat;

        public bool isTriggered;
        
        public override int GetPuzzleSubType() => (int)obstaclePuzzleType;
    }
}
