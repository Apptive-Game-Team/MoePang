using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch
{
    public enum ForcedDirection
    {
        ForcedRow,
        ForcedColumn,
    }
    
    public class ForcedRowColumnPuzzleObject : ObstaclePuzzleObject
    {
        public ForcedDirection forcedDirection = ForcedDirection.ForcedRow;
        private static readonly int HighlightAlphaId = Shader.PropertyToID("_Highlight");
        private Material _material;
        
        private void Start()
        {
            Image img = GetComponent<Image>();
            if (img.material != null)
            {
                _material = new Material(img.material);
                img.material = _material;
            }
        }
        
        public Tween HighlightEffect()
        {
            return DOTween.To(() => 0f, x => _material.SetFloat(HighlightAlphaId, x), 1f, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutCubic);
        }
    }
}
