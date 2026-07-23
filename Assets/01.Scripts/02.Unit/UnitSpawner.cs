using _01.Scripts._11.HabitatMode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _01.Scripts._02.Unit
{
    /// <summary>
    /// 유닛 스폰하는 스크립트
    /// </summary>
    public class UnitSpawner : MonoBehaviour
    {
        [Header("프리팹")]
        [SerializeField] private FriendlyUnit friendlyPrefab;
        [SerializeField] private EnemyUnit enemyPrefab;

        [Header("유닛 리스트")]
        [SerializeField] private FriendlyUnitList friendlyUnitList;
        [SerializeField] private EnemyUnitList enemyUnitList;

        [Header("스폰 위치")]
        [SerializeField] private Transform friendlySpawnPosition;
        [SerializeField] private Transform enemySpawnPosition;
    
        public float enemySpawnInterval = 3f;
        private int enemySpawnCount;

        private Dictionary<Habitat, List<FriendlyUnitData>> _unlockedUnitsByHabitat;

        private void Start()
        {
            enemySpawnCount = 0;
            BuildUnlockedUnitDictionary();

            StartCoroutine(SpawnEnemyCoroutine());
        }

        /// <summary>
        /// 서식지별 해금 유닛 딕셔너리 생성
        /// <para>Scene 새로 들어갈때마다 생성</para>
        /// </summary>
        private void BuildUnlockedUnitDictionary()
        {
            _unlockedUnitsByHabitat = new Dictionary<Habitat, List<FriendlyUnitData>>();

            foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
            {
                List<FriendlyUnitData> unlockedList = new();

                var list = friendlyUnitList.GetUnits(habitat);
                if (list != null)
                {
                    foreach (var unit in list)
                    {
                        if (HabitatManager.Instance.IsUnlocked(unit))
                        {
                            unlockedList.Add(unit);
                        }
                    }
                }

                _unlockedUnitsByHabitat[habitat] = unlockedList;
            }
        }

        /// <summary>
        /// 특정 서식지에서 랜덤 유닛 스폰
        /// </summary>
        public void SpawnFriendly(Habitat habitat)
        {
            if (!_unlockedUnitsByHabitat.TryGetValue(habitat, out var unitList))
            {
                Debug.Log($"{habitat} 서식지 없음");
                return;
            }

            if (unitList.Count == 0)
            {
                Debug.Log($"{habitat} 해금 유닛 없음");
                return;
            }

            FriendlyUnitData data;
        
            //서식지 모드 시 마지막 단계 유닛 소환
            if (HabitatModeManager.Instance.IsHabitatBattle)
            {
                data = unitList[^1];
            }
            else
            {
                data = GetWeightedFriendlyUnit(unitList);
            }

            UnitPool.Instance.Get(friendlyPrefab, data, friendlySpawnPosition);
        }

        public void SpawnHighestFriendly(Habitat habitat)
        {
            if (!_unlockedUnitsByHabitat.TryGetValue(habitat, out var unitList))
            {
                Debug.Log($"{habitat} 서식지 없음");
                return;
            }

            if (unitList.Count == 0)
            {
                Debug.Log($"{habitat} 해금 유닛 없음");
                return;
            }
        
            UnitPool.Instance.Get(friendlyPrefab, unitList[^1], friendlySpawnPosition);
        }

        /// <summary>
        /// 해금별 소환 확률
        /// </summary>
        private FriendlyUnitData GetWeightedFriendlyUnit(List<FriendlyUnitData> unlockedUnits)
        {
            int[] weights = BalanceFormula.GetFriendlySpawnWeights(unlockedUnits);

            int totalWeight = weights.Sum();
            int random = Random.Range(0, totalWeight);

            int cumulative = 0;

            for (int i = 0; i < unlockedUnits.Count; i++)
            {
                cumulative += weights[i];

                if (random < cumulative)
                {
                    return unlockedUnits[i];
                }
            }

            return unlockedUnits[0];
        }

        private void SpawnEnemy()
        {
            var list = enemyUnitList.GetUnits();

            if (list == null || list.Count == 0) return;

            EnemyUnitData data = SetEnemyData(list);

            UnitPool.Instance.Get(enemyPrefab, data, enemySpawnPosition);
        }

        private IEnumerator SpawnEnemyCoroutine()
        {
            yield return new WaitForSeconds(enemySpawnInterval);

            enemySpawnCount++;

            SpawnEnemy();

            if (enemySpawnCount % 5 == 0)
            {
                StartCoroutine(SpawnAdditionalEnemiesCoroutine(2, 0.2f));
            }
        }
        
        private IEnumerator SpawnAdditionalEnemiesCoroutine(int count, float interval)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSeconds(interval);
                SpawnEnemy();
            }
        }

        private EnemyUnitData SetEnemyData(List<EnemyUnitData> list)
        {
            int stage = StageManager.Instance.DifficultyStage;
        
            const int totalWeight = 100;

            int pivot = Random.Range(1, totalWeight + 1);
            int cumulative = 0;

            for (int i = 0; i < list.Count; i++)
            {
                cumulative += BalanceFormula.GetEnemySpawnWeight(i, stage);
                if (pivot <= cumulative)
                {
                    return list[i];
                }
            }

            return list[0];
        }
    }
}
