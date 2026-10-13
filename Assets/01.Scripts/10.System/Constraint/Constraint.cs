using UnityEngine;
using UnityEngine.Localization.Settings;

namespace _01.Scripts._10.System.Constraint
{
    public abstract class Constraint : ScriptableObject
    {
        private const string LocalizationTableName = "LocalizationDataTable";

        public ConstraintType type;
        [TextArea] public string constraintDescription;

        public string LocalizedDescription => LocalizationSettings.StringDatabase.GetLocalizedString(
            LocalizationTableName,
            constraintDescription);

        public abstract void ApplyConstraint(ConstraintContext context);
    }
}
