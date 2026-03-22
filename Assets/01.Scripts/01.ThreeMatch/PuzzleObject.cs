using DG.Tweening;
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
        
        private Vector2 _firstTouchPos;
        private Vector2 _lastTouchPos;
        private bool _isSwapped;

        public abstract int GetPuzzleSubType();
        
        public void Init(PuzzleGenerator generator, int c, int r)
        {
            Generator = generator;
            column = c;
            row = r;
        }
        
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

        public void FailedSwapEffect(int dirX, int dirY, float distance)
        {
            Vector3 dir = new Vector3(dirX, dirY, 0f).normalized;
            Vector3 moveOffset = dir * distance;

            Vector3 originalPos = transform.position;
            Vector3 targetPos = originalPos + moveOffset;

            Sequence seq = DOTween.Sequence();
            float shakeDistance = 0.05f;
            float duration = 0.05f;

            seq.Append(transform.DOMove(targetPos, 0.1f).SetEase(Ease.OutQuad));
            seq.Append(transform.DOMove(originalPos, 0.1f).SetEase(Ease.InQuad));
            for (int i = 0; i < 2; i++)
            {
                seq.Append(transform.DOMoveX(originalPos.x + shakeDistance, duration));
                seq.Append(transform.DOMoveX(originalPos.x - shakeDistance, duration));
            }
            seq.Append(transform.DOMoveX(originalPos.x, duration));
        }
    }
}