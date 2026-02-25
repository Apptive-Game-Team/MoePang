using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    public class NormalPuzzleObject : PuzzleObject
    {
        public NormalPuzzleType normalPuzzleType;
        
        private Vector2 _firstTouchPos;
        private Vector2 _lastTouchPos;

        public override int GetPuzzleSubType() => (int)normalPuzzleType;
        
        private void OnMouseDown()
        {
            _firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
