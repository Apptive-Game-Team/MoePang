using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch.Obstacle
{
    public class ObstaclePuzzleObject : PuzzleObject
    {
        protected static readonly int HighlightAlphaId = Shader.PropertyToID("_Highlight");
        protected Material Material;
        
        public ObstaclePuzzleType obstaclePuzzleType;
        public Habitat habitat;

        public bool isTriggered;
        public bool isMatchable;
        public bool isSwappable;
        
        public override int GetPuzzleSubType() => (int)obstaclePuzzleType;

        public virtual void SetMaterial(Material material)
        {
            Image img = GetComponent<Image>();
            
            Material = new Material(material);
            img.material = Material;
        }
        
        public virtual Tween HighlightEffect()
        {
            return DOTween.To(() => 0f, x => Material.SetFloat(HighlightAlphaId, x), 1f, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutCubic);
        }
    }
}
