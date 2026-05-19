using UnityEngine;

namespace _01.Scripts._10.System.Constraint
{
    [CreateAssetMenu (menuName = "Constraint/BanContinuousHabitat", fileName = "BanContinuousHabitat")]
    public class BanContinuousHabitat : Constraint
    {
        public override void ApplyConstraint(ConstraintContext context)
        {
            Debug.Log("BanContinuousHabitat");
            context.Puzzle.isContinuousHabitatBanned = true;
        }
    }
}
