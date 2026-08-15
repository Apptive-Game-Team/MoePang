using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch.Obstacle
{
    public enum ForcedDirection
    {
        ForcedRow,
        ForcedColumn,
    }
    
    public class ForcedRowColumnPuzzleObject : ObstaclePuzzleObject
    {
        public ForcedDirection forcedDirection = ForcedDirection.ForcedRow;
        
        private void Start()
        {
            Image img = GetComponent<Image>();
            if (img.material != null)
            {
                Material = new Material(img.material);
                img.material = Material;
            }
        }
    }
}
