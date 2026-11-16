using UnityEngine;
using NotImplementedException = System.NotImplementedException;

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

        public override void CancelConstraint(ConstraintContext context)
        {
            context.Puzzle.maxSwapCount = -1;
        }
    }
}
