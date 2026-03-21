using UnityEngine;

/// <summary>
/// 유닛 스폰하는 스크립트
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("유닛 풀")]
    [SerializeField] private UnitPool unitPool;

    [Header("스폰 유닛 데이터")]
    [SerializeField] private UnitData friendlyUnitData;
    [SerializeField] private UnitData enemyUnitData;

    [Header("스폰 위치")]
    [SerializeField] private Transform friendlySpawnPosition;
    [SerializeField] private Transform enemySpawnPosition;

    [Header("스폰 설정")]
    [SerializeField] private float spawnInterval = 10f;

    [ContextMenu("Spawn")]
    /// <summary>
    /// 자동 스폰
    /// </summary>
    private void Spawn()
    {
        unitPool.Get(friendlyUnitData, friendlySpawnPosition);
        //unitPool.Get(enemyUnitData, enemySpawnPosition);
    }

    public void FriendlySpawn()
    {
        unitPool.Get(friendlyUnitData, friendlySpawnPosition);
    }
}
