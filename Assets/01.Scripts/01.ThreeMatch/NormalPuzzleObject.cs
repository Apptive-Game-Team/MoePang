using UnityEngine;

namespace _01.Scripts._01.ThreeMatch
{
    public class NormalPuzzleObject : PuzzleObject
    {
        public NormalPuzzleType normalPuzzleType;
        
        private Vector2 _firstTouchPos;
        private Vector2 _lastTouchPos;
        private bool _isSwapped;

        public override int GetPuzzleSubType() => (int)normalPuzzleType;
        
        private void OnMouseDown()
        {
            _firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _isSwapped = false;
        }
        
        private void OnMouseDrag()
        {
            if (_isSwapped)
            {
                return;
            }

            _lastTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float distance = Vector2.Distance(_firstTouchPos, _lastTouchPos);
            
            if (distance > 0.5f) 
            {
                CalculateSwap();
                _isSwapped = true;
            }
        }

        private void CalculateSwap()
        {
            float swipeAngle = Mathf.Atan2(_lastTouchPos.y - _firstTouchPos.y, _lastTouchPos.x - _firstTouchPos.x) * Mathf.Rad2Deg;
            
            if (Vector2.Distance(_firstTouchPos, _lastTouchPos) < 0.1f) return;

            switch (swipeAngle)
            {
                case > -45 and <= 45:
                    Swap(1, 0);
                    break;
                case > 45 and <= 135:
                    Swap(0, 1);
                    break;
                case > 135 or <= -135:
                    Swap(-1, 0);
                    break;
                case < -45 and >= -135:
                    Swap(0, -1);
                    break;
            }
        }

        private void Swap(int dirX, int dirY)
        {
            Generator.TrySwapPuzzles(column, row, column + dirX, row + dirY);
        }
    }
}
