using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch
{
    public class NormalPuzzleObject : PuzzleObject
    {
        public NormalPuzzleType normalPuzzleType;
        
        private Vector2 _firstTouchPos;
        private Vector2 _lastTouchPos;
        private bool _isSwapped;

        private Material _material;
        private static readonly int HighlightAlphaId = Shader.PropertyToID("_Highlight");

        public override int GetPuzzleSubType() => (int)normalPuzzleType;

        private void Start()
        {
            Image img = GetComponent<Image>();
            if (img.material != null)
            {
                _material = new Material(img.material);
                img.material = _material;
            }
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

        public Tween HighlightEffect()
        {
            return DOTween.To(() => 0f, x => _material.SetFloat(HighlightAlphaId, x), 1f, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutCubic);
        }
    }
}
