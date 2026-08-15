using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch.Obstacle
{
    public class FixedPuzzleObject : ObstaclePuzzleObject
    {
        public override void SetMaterial(Material material)
        {
            Image img = transform.GetChild(2).GetComponent<Image>();
            
            Material = new Material(material);
            img.material = Material;
        }
    }
}
