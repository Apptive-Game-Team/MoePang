using UnityEngine;

namespace _01.Scripts._10.System.Constraint
{
    [CreateAssetMenu (menuName = "Constraint/SwapCountConstraint", fileName = "SwapCountConstraint")]
    public class SwapCountConstraint : Constraint
    {
        public int maxSwapCount;
        
        public override void ApplyConstraint(ConstraintContext context)
        {
            Debug.Log("SwapCountConstraint");
            context.Puzzle.maxSwapCount = maxSwapCount;
        }
    }
}
