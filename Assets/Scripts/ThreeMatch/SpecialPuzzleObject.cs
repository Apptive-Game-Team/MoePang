using System;
using UnityEngine;

namespace ThreeMatch
{
    public class SpecialPuzzleObject : PuzzleObject
    {
        public SpecialPuzzleType specialPuzzleType;
        public NormalPuzzleType colorBombType;

        private float _lastClickTime = 0f;
        private const float DoubleClickThreshold = 0.3f;

        private bool _progressing;

        public override int GetPuzzleSubType() => (int)specialPuzzleType;

        private void OnMouseDown()
        {
            if (Generator.IsProcessing) return;
            
            float timeSinceLastClick = Time.time - _lastClickTime;

            if (timeSinceLastClick <= DoubleClickThreshold && !_progressing)
            {
                _progressing = true;
                Generator.StartCoroutine(Generator.ActivateSpecialBomb(column, row, specialPuzzleType));
            }
            
            _lastClickTime = Time.time;
        }
    }
}
