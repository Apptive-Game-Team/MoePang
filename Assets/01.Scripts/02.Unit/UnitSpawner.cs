using UnityEngine;

/// <summary>
/// 유닛 스폰하는 스크립트
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("유닛 풀")]
    [SerializeField] private UnitPool friendlyPool;
    [SerializeField] private UnitPool enemyPool;

    [Header("스폰 설정")]
    [SerializeField] private Transform friendlySpawnPosition;
    [SerializeField] private Transform enemySpawnPosition;
    [SerializeField] private float spawnInterval = 10.0f;

    //참조
    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }

    /// <summary>
    /// 유닛 소환
    /// </summary>
    private void Spawn()
    {
        friendlyPool.SpawnUnit(friendlySpawnPosition);
        enemyPool.SpawnUnit(enemySpawnPosition);
    }

    public void FriendlySpawn()
    {
        friendlyPool.SpawnUnit(friendlySpawnPosition);
    }
}
