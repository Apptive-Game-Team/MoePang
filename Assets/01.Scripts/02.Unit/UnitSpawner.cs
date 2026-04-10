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

    private Dictionary<Habitat, List<FriendlyUnitData>> _unlockedUnitsByHabitat;

    private void Awake()
    {
        _enemySpawnWeights = new List<List<int>>()
        {
            new(){70, 30, 0},
            new(){60, 30, 10},
        };
    }

    private void Start()
    {
        BuildUnlockedUnitDictionary();

        StartCoroutine(SpawnEnemyCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SpawnFriendly(Habitat.Meadow);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnEnemy();
        }
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

        FriendlyUnitData data = GetWeightedFriendlyUnit(unitList);

        UnitPool.Instance.Get(friendlyPrefab, data, friendlySpawnPosition);
    }

    /// <summary>
    /// 해금별 소환 확률
    /// </summary>
    private FriendlyUnitData GetWeightedFriendlyUnit(List<FriendlyUnitData> unlockedUnits)
    {
        int count = unlockedUnits.Count;

        // 1개면 하나만
        if (count == 1)
        {
            return unlockedUnits[0];
        }

        // 2개면 60 / 40
        if (count == 2)
        {
            int roll = UnityEngine.Random.Range(0, 100);

            return roll < 60
                ? unlockedUnits[0]
                : unlockedUnits[1];
        }

        // 3개 이상이면 최근 3개 60/ 30 / 10
        List<FriendlyUnitData> recentThree = unlockedUnits
            .Skip(count - 3)
            .Take(3)
            .ToList();

        int[] weights = { 60, 30, 10 };
        int random = UnityEngine.Random.Range(0, 100);

        int cumulative = 0;
        for (int i = 0; i < recentThree.Count; i++)
        {
            cumulative += weights[i];

            if (random < cumulative)
            {
                return recentThree[i];
            }
        }

        return recentThree[0];
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

        if (stage >= 50)
        {
            return list[^1];
        }
        
        int cycleStep = stage % 10;
        int subIndex = (cycleStep < 3) ? 0 : (cycleStep < 6 ? 1 : 2);
        int index = stage / 10 * 3 + subIndex;

        int enemyStart = index / 2;
        int enemyWeightsStep = index % 2;
        
        List<int> enemyWeight = _enemySpawnWeights[enemyWeightsStep];

        int totalWeight = enemyWeight.Sum();

        int pivot = Random.Range(1, totalWeight + 1);
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
