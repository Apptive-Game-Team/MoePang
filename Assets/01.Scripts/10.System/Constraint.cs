using UnityEngine;

namespace _01.Scripts._10.System
{
    [CreateAssetMenu (menuName = "Constraint", fileName = "Constraint")]
    public abstract class Constraint : ScriptableObject
    {
        public ConstraintType type;
        [SerializeField] private string constraintName;
        [SerializeField, TextArea] private string constraintDescription;

        public abstract void ApplyConstraint(ConstraintContext context);
    }
}
