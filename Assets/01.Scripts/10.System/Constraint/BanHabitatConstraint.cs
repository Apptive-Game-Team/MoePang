using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _01.Scripts._10.System.Constraint
{
    [CreateAssetMenu (menuName = "Constraint/BanHabitatConstraint", fileName = "BanHabitatConstraint")]
    public class BanHabitatConstraint : Constraint
    {
        public Habitat banHabitat;
        
        public override void ApplyConstraint(ConstraintContext context)
        {
            banHabitat = (Habitat)Random.Range(0, Enum.GetValues(typeof(Habitat)).Length);
            context.SpawnStacks.First(s => s.type == banHabitat).BanStack();
            
            Debug.Log($"BanHabitatConstraint {banHabitat}");
        }
    }
}
