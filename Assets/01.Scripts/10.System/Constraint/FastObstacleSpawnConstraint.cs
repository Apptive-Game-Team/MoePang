using UnityEngine;

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
    }
}
