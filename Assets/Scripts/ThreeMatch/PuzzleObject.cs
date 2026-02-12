using UnityEngine;

namespace ThreeMatch
{
    public class PuzzleObject : MonoBehaviour
    {
        public PuzzleType puzzleType;
        public int column, row;
        public bool isMatched;
        
        protected PuzzleGenerator Generator;

        public void Init(PuzzleGenerator generator, int c, int r)
        {
            Generator = generator;
            column = c;
            row = r;
        }
    }
}