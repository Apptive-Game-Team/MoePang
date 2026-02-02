using UnityEngine;

/// <summary>
/// 유닛 스폰하는 스크립트
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private UnitPool unitPool;

    [Header("설정")]
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private float spawnInterval = 10.0f;

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
        unitPool.SpawnUnit(spawnPosition);
    }

}
