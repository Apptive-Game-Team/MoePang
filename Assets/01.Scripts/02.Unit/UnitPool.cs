using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unit Object Pool
/// </summary>
public class UnitPool : MonoBehaviour
{
    public static UnitPool Instance;

    [Header("Pool Setting")]
    [SerializeField] private Unit unitPrefab;

    private Dictionary<UnitInFo, Queue<Unit>> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Unit 가져오기
    /// </summary>
    public Unit Get(UnitInFo data, Transform spawnPos)
    {
        if (!pools.ContainsKey(data))
        {
            pools.Add(data, new Queue<Unit>());
        }

        Unit unit;

        if (pools[data].Count > 0)
        {
            unit = pools[data].Dequeue();
        }
        else
        {
            unit = Instantiate(unitPrefab, transform);
            unit.SetPool(this);
        }

        unit.transform.position = spawnPos.position;
        unit.SetData(data);
        unit.gameObject.SetActive(true);

        return unit;
    }

    /// <summary>
    /// Unit 반환
    /// </summary>
    public void ReturnUnit(Unit unit)
    {
        unit.gameObject.SetActive(false);

        UnitInFo data = unit.Data;

        if (!pools.ContainsKey(data))
        {
            pools.Add(data, new Queue<Unit>());
        }

        pools[data].Enqueue(unit);
    }
}