using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    public enum PuzzleState
    {
        Idle,
        Falling,
        Swapping,
        Matching,
        Spawn
    }
    
    public abstract class PuzzleObject : MonoBehaviour
    {
        public PuzzleType puzzleType;
        public PuzzleState puzzleState = PuzzleState.Spawn;
        public int column, row;
        public bool isMatched;
        
        protected PuzzleGenerator Generator;

        public abstract int GetPuzzleSubType();
        
        public void Init(PuzzleGenerator generator, int c, int r)
        {
            Generator = generator;
            column = c;
            row = r;
        }
    }
}