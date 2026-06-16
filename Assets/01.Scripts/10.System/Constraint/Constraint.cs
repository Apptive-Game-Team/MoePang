using UnityEngine;

namespace _01.Scripts._10.System.Constraint
{
    public abstract class Constraint : ScriptableObject
    {
        public ConstraintType type;
        [TextArea] public string constraintDescription;

        public abstract void ApplyConstraint(ConstraintContext context);
    }
}
