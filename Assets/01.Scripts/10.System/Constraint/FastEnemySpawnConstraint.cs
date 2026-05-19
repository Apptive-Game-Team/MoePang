using UnityEngine;

namespace _01.Scripts._10.System.Constraint
{
    [CreateAssetMenu (menuName = "Constraint/FastEnemySpawnConstraint", fileName = "FastEnemySpawnConstraint")]
    public class FastEnemySpawnConstraint : Constraint
    {
        [SerializeField] private float enemySpawnDecrease;
        
        public override void ApplyConstraint(ConstraintContext context)
        {
            Debug.Log("FastEnemySpawnConstraint");
            context.UnitSpawner.enemySpawnInterval -= enemySpawnDecrease;
        }
    }
}
