using UnityEngine;

namespace ThreeMatch
{
    public class PuzzleObject : MonoBehaviour
    {
        public PuzzleGenerator.PuzzleType puzzleType;
        public bool isMatched;
        public int column, row;

        private Vector2 _firstTouchPos;
        private Vector2 _lastTouchPos;
        private PuzzleGenerator _generator;

        public void Init(PuzzleGenerator generator, int c, int r)
        {
            _generator = generator;
            column = c;
            row = r;
        }

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
            _generator.TrySwapPuzzles(column, row, column + dirX, row + dirY);
        }
    }
}