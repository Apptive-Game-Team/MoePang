using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using _01.Scripts._02.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _01.Scripts._10.System.Combo
{
    public class ComboContext
    {
        public PuzzleGenerator Puzzle { get; private set; }
        public UnitSpawner UnitSpawner { get; private set; }
        public SpawnStack[] SpawnStacks { get; private set; }

        public ComboContext(PuzzleGenerator puzzle, UnitSpawner unitSpawner, SpawnStack[] spawnStacks)
        {
            Puzzle = puzzle;
            UnitSpawner = unitSpawner;
            SpawnStacks = spawnStacks;
        }
    }
    
    public class ComboSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpawnStack[] stacks;
        [SerializeField] private UnitSpawner unitSpawner;
        [SerializeField] private List<Combo> combos;
        
        public bool isTypeNull;
        
        private int _continuousComboCount;
        private PuzzleGenerator _puzzle;
        private ComboContext _context;

        private void Awake()
        {
            _puzzle = GetComponent<PuzzleGenerator>();
            
            _context = new ComboContext(_puzzle, unitSpawner, stacks);
        }

        private void Start()
        {
            _puzzle.OnComboInitialized += OnComboInitialized;
            _puzzle.OnComboDetected += OnComboDetected;
        }

        private void OnComboInitialized()
        {
            _continuousComboCount = 0;
        }

        private void OnComboDetected()
        {
            _continuousComboCount++;

            if (_continuousComboCount is < 2 or > 6)
            {
                return;
            }
            
            print($"{GameManager.Instance.comboData.comboSequence[_continuousComboCount - 2]} combo Applied");
            
            combos.First(c => c.info.comboType == GameManager.Instance.comboData.comboSequence[_continuousComboCount - 2]).TriggerComboEffect(_context);
        }
    }
}
