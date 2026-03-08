using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    public class SpecialPuzzleObject : PuzzleObject
    {
        public SpecialPuzzleType specialPuzzleType;
        public NormalPuzzleType colorBombType;

        private Vector2 _firstTouchPos;
        private Vector2 _lastTouchPos;
        private float _lastClickTime = 0f;
        private const float DoubleClickThreshold = 0.3f;

        private bool _progressing;

        public override int GetPuzzleSubType() => (int)specialPuzzleType;

        private void OnMouseDown()
        {
            if (Generator.IsProcessing) return;
            
            // for swap
            _firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // for double click
            float timeSinceLastClick = Time.time - _lastClickTime;
            if (timeSinceLastClick <= DoubleClickThreshold && !_progressing)
            {
                _progressing = true;
                Generator.AddTask(() => Generator.ActivateSpecialBomb(column, row, specialPuzzleType));
            }
            _lastClickTime = Time.time;
        }
        
        private void OnMouseUp()
        {
            _lastTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateSwipe();
        }

        private void CalculateSwipe()
        {
            float swipeAngle = Mathf.Atan2(_lastTouchPos.y - _firstTouchPos.y, _lastTouchPos.x - _firstTouchPos.x) * Mathf.Rad2Deg;
            
            if (Vector2.Distance(_firstTouchPos, _lastTouchPos) < 0.1f) return;

            if (swipeAngle > -45 && swipeAngle <= 45) Swap(1, 0);
            else if (swipeAngle > 45 && swipeAngle <= 135) Swap(0, 1);
            else if (swipeAngle > 135 || swipeAngle <= -135) Swap(-1, 0);
            else if (swipeAngle < -45 && swipeAngle >= -135) Swap(0, -1);
        }

        private void Swap(int dirX, int dirY)
        {
            Generator.TrySwapPuzzles(column, row, column + dirX, row + dirY);
        }
    }
}
