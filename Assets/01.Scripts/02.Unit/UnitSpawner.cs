using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using _01.Scripts._11.HabitatMode;

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
    public List<List<int>> friendlySpawnWeights;
    public float enemySpawnInterval = 3f;

    private Dictionary<Habitat, List<FriendlyUnitData>> _unlockedUnitsByHabitat;

    private void Awake()
    {
        _enemySpawnWeights = StageManager.Instance.CurrentStage < 50 ?
            new List<List<int>>()
            {
                new(){70, 30, 0},
                new(){50, 30, 20},
            } : 
            new List<List<int>>()
            {
                new() { 18, 16, 14, 13, 12, 10, 8, 6, 3 },
                new() { 14, 14, 13, 13, 12, 11, 10, 8, 5 },
                new() { 10, 11, 11, 12, 12, 12, 12, 10, 10 },
                new() { 7, 8, 9, 10, 11, 12, 13, 14, 16 },
                new() { 4, 5, 6, 7, 9, 11, 14, 18, 26 }
            };

        friendlySpawnWeights = StageManager.Instance.CurrentStage < 50 ? 
            new List<List<int>>() 
            {
                new() { 100 },
                new() { 60, 40 },
                new() { 60, 30, 10 }
            } : 
            new List<List<int>>() 
            {
                new() { 100 },
                new() { 70, 30 },
                new() { 60, 30, 10 },
                new() { 50, 30, 15, 5 },
                new() { 35, 25, 18, 14, 8 }, 
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
        bool isTutorial = StageManager.Instance.CurrentStage < 50;
        
        int count = isTutorial ? Mathf.Min(3, unlockedUnits.Count) : unlockedUnits.Count;

        List<FriendlyUnitData> recent = isTutorial
            ? unlockedUnits.Count > count
                ? unlockedUnits.Skip(unlockedUnits.Count - count).Take(count).ToList()
                : unlockedUnits
            : unlockedUnits;

        List<int> weights = friendlySpawnWeights[count - 1];
        int random = Random.Range(0, 100);

        int cumulative = 0;
        for (int i = 0; i < recent.Count; i++)
        {
            cumulative += weights[i];

            if (random < cumulative)
            {
                return recent[i];
            }
        }

        return recent[0];
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
            yield return new WaitForSeconds(enemySpawnInterval);
            SpawnEnemy();
        }
    }

    private EnemyUnitData SetEnemyData(List<EnemyUnitData> list)
    {
        int stage = StageManager.Instance.DifficultyStage;

        if (stage >= 50)
        {
            if (stage < 100)
            {
                List<int> weight = _enemySpawnWeights[stage / 10 - 5];
                
                int totalWeight = weight.Sum();
                int pivot = Random.Range(1, totalWeight + 1);
                int cumulative = 0;

                for (int i = 0; i < weight.Count; i++)
                {
                    cumulative += weight[i];
                    if (pivot <= cumulative)
                    {
                        int targetIndex = Mathf.Clamp(i, 0, list.Count - 1);
                        return list[targetIndex];
                    }
                }

                return list[0];
            }
            else
            {
                
            }
        }

        {
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
}
