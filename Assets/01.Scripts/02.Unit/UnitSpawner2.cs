using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class UnitSpawner2 : MonoBehaviour
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
            new(){40, 30, 30}
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

        FriendlyUnitData data = unitList[UnityEngine.Random.Range(0, unitList.Count)];

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

        int pivot = UnityEngine.Random.Range(0, totalWeight + 1);
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
