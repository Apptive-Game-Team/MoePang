using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 유닛 오브젝트 풀
/// </summary>
public class UnitPool : MonoBehaviour
{
    [Header("풀 설정")]
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private int poolCount = 10;

    //오브젝트 풀링할 큐
    private Queue<Unit> pool = new Queue<Unit>();

    private void Awake()
    {
        for (int i = 0; i < poolCount; i++)
        {
            CreateUnit();
        }
    }

    /// <summary>
    /// 풀에 유닛 생성
    /// </summary>
    /// <returns></returns>
    private Unit CreateUnit()
    {
        Unit unit = Instantiate(unitPrefab, transform);
        unit.gameObject.SetActive(false);
        unit.SetPool(this);
        pool.Enqueue(unit);
        return unit;
    }
    /// <summary>
    /// 풀에서 유닛 꺼내오기
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <returns></returns>
    public Unit SpawnUnit (Transform spawnPos)
    {
        if (pool.Count == 0)
        {
            CreateUnit();
        }

        Unit unit = pool.Dequeue();
        unit.transform.position = spawnPos.position;
        unit.gameObject.SetActive(true);
        return unit;
    }
    /// <summary>
    /// 풀에 유닛 반환
    /// </summary>
    /// <param name="unit"></param>
    public void ReturnUnit(Unit unit)
    {
        unit.gameObject.SetActive(false);
        pool.Enqueue(unit);
    }
}
