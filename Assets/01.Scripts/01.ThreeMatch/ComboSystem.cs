using _01.Scripts._00.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace _01.Scripts._01.ThreeMatch
{
    public class ComboSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpawnStack[] stacks;
        [SerializeField] private UnitSpawner unitSpawner;
        
        public bool isTypeNull;
        
        private int _continuousComboCount;
        private PuzzleGenerator _puzzle;

        private void Awake()
        {
            _puzzle = GetComponent<PuzzleGenerator>();
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

            switch (_continuousComboCount)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    print($"{_continuousComboCount} combo detected");
                    TriggerComboEffect(GameManager.Instance.comboData.comboSequence[_continuousComboCount - 2]);
                    break;
            }
        }

        private void TriggerComboEffect(Habitat comboType)
        {
            print($"{comboType} combo applied");
            
            // if (isTypeNull)
            // {
            //     // 모든 속성 1스택 추가
            //     foreach (SpawnStack stack in stacks)
            //     {
            //         stack.AddStack(1);
            //     }
            //
            //     return;
            // }
            
            switch (comboType)
            {
                case Habitat.Meadow:
                    // 모든 서식지 기물 1종씩 소환
                    foreach (Habitat type in Enum.GetValues(typeof(Habitat)))
                    {
                        unitSpawner.SpawnFriendly(type);
                    }
                    break;
                case Habitat.Desert:
                    // 행, 열폭탄 생성
                    _puzzle.SpawnSpecialPuzzle(SpecialPuzzleType.ColumnBomb);
                    _puzzle.SpawnSpecialPuzzle(SpecialPuzzleType.RowBomb);
                    break;
                case Habitat.Forest:
                    // 모든 아군유닛 체력회복
                    FriendlyUnit[] allies = FindObjectsByType<FriendlyUnit>(FindObjectsSortMode.None);
                    foreach (FriendlyUnit ally in allies)
                    {
                        ally.Heal(10000f);
                    }
                    break;
                case Habitat.Ocean:
                    // 잠시 아군의 공격속도, 이동속도, 공격력 증가
                    BuffManager.Instance.ApplyAllyBuff(StatType.AttackDamage, 1.2f, 5f);
                    BuffManager.Instance.ApplyAllyBuff(StatType.AttackSpeed, 1.2f, 5f);
                    BuffManager.Instance.ApplyAllyBuff(StatType.MoveSpeed, 1.2f, 5f);
                    break;
                case Habitat.Polar:
                    // 잠시 적 이동속도, 공격속도 감소
                    BuffManager.Instance.ApplyEnemyBuff(StatType.AttackSpeed, 0.8f, 5f);
                    BuffManager.Instance.ApplyEnemyBuff(StatType.MoveSpeed, 0.8f, 5f);
                    break;
            }
        }
    }
}
