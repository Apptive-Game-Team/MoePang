using _01.Scripts._01.ThreeMatch;
using _01.Scripts._02.Unit;
using _01.Scripts._11.HabitatMode;
using DG.Tweening;
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
        [SerializeField] private TextMeshProUGUI highConstraintText;
        
        [Header("Constraint Settings")]
        [SerializeField] private int constraintApplyInterval;
        private ConstraintContext _constraintContext;
        private ConstraintRouletteSystem _constraintRouletteSystem;

        private void Awake()
        {
            _constraintContext = new ConstraintContext(puzzle, unitSpawner, spawnStacks);
            _constraintRouletteSystem = GetComponent<ConstraintRouletteSystem>();
            _constraintRouletteSystem.InitializeItems();

            bool setMiddleConstraint = StageManager.Instance.IsMiddleHurdleStage();
            if (setMiddleConstraint)
            {
                StartCoroutine(StartRoulette());
            }

            bool setHighConstraint = StageManager.Instance.IsHighHurdleStage();
            if (setHighConstraint)
            {
                StartCoroutine(ApplyHighConstraint());
            }
        }

        private IEnumerator StartRoulette()
        {
            rouletteObject.SetActive(true);
            Time.timeScale = 0f;
            yield return _constraintRouletteSystem.StartRoulette(ApplyMiddleConstraint);
            yield return new WaitForSecondsRealtime(1f);
            Time.timeScale = 1f;
            rouletteObject.SetActive(false);
        }

        private void ApplyMiddleConstraint(ConstraintType type)
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

        private IEnumerator ApplyHighConstraint()
        {
            Time.timeScale = 0f;
            
            HabitatMode type = (HabitatMode)(StageManager.Instance.CurrentStage / 10);
            SetHighConstraintText(type);
            highConstraintText.transform.parent.gameObject.SetActive(true);
            
            CanvasGroup cg = highConstraintText.GetComponentInParent<CanvasGroup>();
            Sequence sq = DOTween.Sequence();
            sq.SetUpdate(true);
            sq.Append(cg.DOFade(1f, 0.5f));
            sq.Append(cg.DOFade(0f, 0.5f));
            sq.Append(cg.DOFade(1f, 0.5f));
            sq.Append(cg.DOFade(0f, 0.5f));
            sq.Append(cg.DOFade(1f, 0.5f));

            yield return new WaitForSecondsRealtime(3f);
            
            HabitatModeManager.Instance.habitatMode = type;
            HabitatModeManager.Instance.ApplyHabitatModeEffect();

            highConstraintText.transform.parent.gameObject.SetActive(false);
            Time.timeScale = 1f;
        }

        private void SetHighConstraintText(HabitatMode mode)
        {
            switch (mode)
            {
                case HabitatMode.MeadowMode:
                    highConstraintText.text = "기물을 소환하는데 더 많은 스택을 필요로 합니다.";
                    break;
                case HabitatMode.OceanMode:
                    highConstraintText.text = "적의 공격속도, 공격력, 이동속도가 증가합니다.";
                    break;
                case HabitatMode.DesertMode:
                    highConstraintText.text = "일정 시간마다 모래바람이 불어 퍼즐 테이블이 잠시 가려집니다.";
                    break;
                case HabitatMode.ForestMode:
                    highConstraintText.text = "적의 체력이 일정 시간마다 회복됩니다.";
                    break;
                case HabitatMode.PolarMode:
                    highConstraintText.text = "아군의 이동속도와 공격속도가 느려집니다.";
                    break;
            }
        }
    }
}
