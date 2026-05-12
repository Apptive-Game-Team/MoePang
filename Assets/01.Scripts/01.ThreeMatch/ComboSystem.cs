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
        [Header("Combo Settings")]
        [SerializeField] private float comboKeepTime = 5f;
        [SerializeField] private int comboThreshold = 21;
        
        [Header("Castle Settings")]
        public Habitat castleType;
        public bool isTypeNull;
        
        [Header("References")]
        [SerializeField] private SpawnStack[] stacks;
        [SerializeField] private UnitSpawner unitSpawner;
        
        private List<(int amount, float time)> _comboTrackers = new();
        private PuzzleGenerator _puzzle;

        private void Awake()
        {
            _puzzle = GetComponent<PuzzleGenerator>();
        }

        private void Start()
        {
            foreach (SpawnStack stack in stacks)
            {
                stack.OnStackAdded += OnStackAdded;
            }
        }
        
        private void OnStackAdded(int amount)
        {
            float currentTime = Time.time;
            
            _comboTrackers.Add((amount, currentTime));
            
            _comboTrackers.RemoveAll(x => currentTime - x.time > comboKeepTime);
            int totalRecentStacks = _comboTrackers.Sum(x => x.amount);
            
            if (totalRecentStacks >= comboThreshold)
            {
                TriggerComboEffect();
                _comboTrackers.Clear();
            }
        }

        private void TriggerComboEffect()
        {
            print("콤보 발동");
            
            if (isTypeNull)
            {
                // 모든 속성 1스택 추가
                foreach (SpawnStack stack in stacks)
                {
                    stack.AddStack(1);
                }

                return;
            }
            
            switch (castleType)
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
