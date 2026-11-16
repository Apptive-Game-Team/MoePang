using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace _01.Scripts._10.System.Constraint
{
    [CreateAssetMenu (menuName = "Constraint/FastObstacleSpawnConstraint", fileName = "FastObstacleSpawnConstraint")]
    public class FastObstacleSpawnConstraint : Constraint
    {
        [SerializeField] private float obstacleSpawnDecrease;
        
        public override void ApplyConstraint(ConstraintContext context)
        {
            Debug.Log("FastObstacleSpawnConstraint");
            context.Puzzle.obstacleSpawnInterval -= obstacleSpawnDecrease;
        }

        public override void CancelConstraint(ConstraintContext context)
        {
            context.Puzzle.obstacleSpawnInterval += obstacleSpawnDecrease;
        }
    }
}
