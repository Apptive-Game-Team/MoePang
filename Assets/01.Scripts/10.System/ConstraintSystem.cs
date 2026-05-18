using _01.Scripts._01.ThreeMatch;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _01.Scripts._10.System
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
        [SerializeField] private int constraintApplyCount;
        private ConstraintContext _constraintContext;

        private void Start()
        {
            _constraintContext = new ConstraintContext(puzzle, unitSpawner, spawnStacks);
        }

        public void ApplyConstraint(ConstraintType type)
        {
            constraints.First(c => c.type == type).ApplyConstraint(_constraintContext);
        }
    }
}
