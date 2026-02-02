using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 유닛 오브젝트 풀
/// </summary>
public class UnitPool : MonoBehaviour
{
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private int poolCount = 10;

    private Queue<Unit> pool = new Queue<Unit>();

    private void Awake()
    {
        for (int i = 0; i < poolCount; i++)
        {
            CreateUnit();
        }
    }

    private Unit CreateUnit()
    {
        Unit unit = Instantiate(unitPrefab, transform);
        unit.gameObject.SetActive(false);
        unit.SetPool(this);
        pool.Enqueue(unit);
        return unit;
    }
    public Unit SpawnUnit (Vector3 position)
    {
        if (pool.Count == 0)
        {
            CreateUnit();
        }

        Unit unit = pool.Dequeue();
        unit.transform.position = position;
        unit.gameObject.SetActive(true);
        return unit;
    }
    public void ReturnUnit(Unit unit)
    {
        unit.gameObject.SetActive(false);
        pool.Enqueue(unit);
    }
}
