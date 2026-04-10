using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

    private List<List<int>> _enemySpawnWeights;
    private float _enemySpawnInterval = 3f;

    private void Awake()
    {
        _enemySpawnWeights = new List<List<int>>()
        {
            new(){70, 30, 0},
            new(){60, 30, 10},
            new(){40, 30, 30}
        };
    }
    
    private void Start()
    {
        StartCoroutine(SpawnEnemyCoroutine());
    }

    public void SpawnFriendly()
    {
        List<FriendlyUnitData> unlockedUnits = new();

        foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
        {
            var list = friendlyUnitList.GetUnits(habitat);
            if (list == null) continue;

            foreach (var unit in list)
            {
                if (HabitatManager.Instance.IsUnlocked(unit))
                {
                    unlockedUnits.Add(unit);
                }
            }
        }

        if (unlockedUnits.Count == 0)
        {
            Debug.Log("해금된 유닛 없음");
            return;
        }

        var data = unlockedUnits[Random.Range(0, unlockedUnits.Count)];

        UnitPool.Instance.Get(friendlyPrefab, data, friendlySpawnPosition);
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
        while (true)
        {
            yield return new WaitForSeconds(_enemySpawnInterval);
            SpawnEnemy();
        }
    }

    private EnemyUnitData SetEnemyData(List<EnemyUnitData> list)
    {
        int stage = StageManager.Instance.CurrentStage;
        
        if (stage >= 40)
        {
            return list[^1];
        }

        int enemyRange = _enemySpawnWeights[0].Count;
        int enemyWeightsStep = Math.Clamp((stage % 10) / enemyRange, 0, enemyRange - 1);
        int enemyStart = stage / 10 * 2;
        List<int> enemyWeight = _enemySpawnWeights[enemyWeightsStep];

        int totalWeight = enemyWeight.Sum();

        int pivot = Random.Range(0, totalWeight + 1);
        int cumulative = 0;

        for (int i = 0; i < enemyWeight.Count; i++)
        {
            cumulative += enemyWeight[i];
            if (pivot <= cumulative)
            {
                int targetIndex = Mathf.Clamp(enemyStart + i, 0, list.Count - 1);
                return list[targetIndex];
            }
        }

        return list[enemyStart];
    }
}
