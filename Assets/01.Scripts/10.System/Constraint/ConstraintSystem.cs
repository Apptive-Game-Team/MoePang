using _01.Scripts._01.ThreeMatch;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _01.Scripts._10.System.Constraint
{
    public enum ConstraintType
    {
        SwapCount,
        BanHabitat,
        FastObstacleSpawn,
        FastEnemySpawn,
        RestartPuzzle,
        BanContinuousHabitat,
    }

    public class ConstraintContext
    {
        public PuzzleGenerator Puzzle { get; private set; }
        public UnitSpawner UnitSpawner { get; private set; }
        public SpawnStack[] SpawnStacks { get; private set; }

        public ConstraintContext(PuzzleGenerator puzzle, UnitSpawner unitSpawner, SpawnStack[] spawnStacks)
        {
            Puzzle = puzzle;
            UnitSpawner = unitSpawner;
            SpawnStacks = spawnStacks;
        }
    }
    
    public class ConstraintSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<Constraint> constraints;
        [SerializeField] private PuzzleGenerator puzzle;
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private SpawnStack[] spawnStacks;
        
        [Header("Constraint Settings")]
        [SerializeField] private int constraintApplyInterval;
        private int _constraintApplyCount;
        private ConstraintContext _constraintContext;

        private void Start()
        {
            _constraintContext = new ConstraintContext(puzzle, unitSpawner, spawnStacks);

            _constraintApplyCount = StageManager.Instance.CurrentStage / constraintApplyInterval;

            ApplyRandomConstraints();
        }
        
        private void ApplyRandomConstraints()
        {
            ConstraintType[] allTypes = (ConstraintType[])Enum.GetValues(typeof(ConstraintType));
            
            int targetCount = Mathf.Min(_constraintApplyCount, allTypes.Length);

            if (targetCount <= 0)
            {
                return;
            }
            
            var randomTypes = allTypes
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(targetCount);
            
            foreach (ConstraintType type in randomTypes)
            {
                ApplyConstraint(type);
            }
        }

        private void ApplyConstraint(ConstraintType type)
        {
            constraints.First(c => c.type == type).ApplyConstraint(_constraintContext);
        }
    }
}
