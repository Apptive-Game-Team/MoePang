using _01.Scripts._01.ThreeMatch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private GameObject rouletteObject;
        [SerializeField] private TextMeshProUGUI constraintText;
        [SerializeField] private Image constraintImage;
        [SerializeField] private TextMeshProUGUI constraintCount;
        
        [Header("Constraint Settings")]
        [SerializeField] private int constraintApplyInterval;
        private ConstraintContext _constraintContext;
        private ConstraintRouletteSystem _constraintRouletteSystem;

        private void Start()
        {
            _constraintContext = new ConstraintContext(puzzle, unitSpawner, spawnStacks);
            _constraintRouletteSystem = GetComponent<ConstraintRouletteSystem>();

            StartCoroutine(StartRoulette());
        }

        private IEnumerator StartRoulette()
        {
            rouletteObject.SetActive(true);
            yield return _constraintRouletteSystem.StartRoulette(ApplyConstraint);
            yield return new WaitForSeconds(1f);
            rouletteObject.SetActive(false);
        }

        private void ApplyConstraint(ConstraintType type)
        {
            Constraint constraint = constraints.First(c => c.type == type);
            constraint.ApplyConstraint(_constraintContext);
            constraintText.text = constraint.constraintDescription;
            constraintText.transform.parent.gameObject.SetActive(true);

            switch (constraint)
            {
                case BanHabitatConstraint bc:
                    constraintImage.sprite = puzzle.normalPuzzleImages[(int)bc.banHabitat];
                    constraintImage.gameObject.SetActive(true);
                    constraintCount.text = "";
                    break;
                case SwapCountConstraint sc:
                    constraintImage.sprite = null;
                    constraintImage.gameObject.SetActive(true);
                    constraintCount.text = sc.maxSwapCount.ToString();
                    puzzle.OnSwapCountChanged += RegisterSwapCountConstraint;
                    break;
            }
        }

        private void RegisterSwapCountConstraint(int count)
        {
            constraintCount.text = count.ToString();
        }
    }
}
