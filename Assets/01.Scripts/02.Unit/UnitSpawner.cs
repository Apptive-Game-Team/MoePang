using System.Collections.Generic;
using UnityEngine;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SpawnFriendly();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnEnemy();
        }
    }

    public void SpawnFriendly()
    {
        List<FriendlyUnitData> unlockedUnits = new();

        foreach (Habitat habitat in System.Enum.GetValues(typeof(Habitat)))
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

        var data = list[Random.Range(0, list.Count)];

        UnitPool.Instance.Get(enemyPrefab, data, enemySpawnPosition);
    }
}
