using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    public abstract class PuzzleObject : MonoBehaviour
    {
        public PuzzleType puzzleType;
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