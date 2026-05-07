using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unit Object Pool
/// </summary>
public class UnitPool : MonoBehaviour
{
    public static UnitPool Instance;

    private Dictionary<Unit, Queue<Unit>> pools = new();

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
    public Unit Get(Unit prefab, UnitData data, Transform spawnPos)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools.Add(prefab, new Queue<Unit>());
        }

        Unit unit;

        if (pools[prefab].Count > 0)
        {
            unit = pools[prefab].Dequeue();
        }

        else
        {
            unit = Instantiate(prefab, transform);
            unit.SetPool(this);
            unit.SetOriginPrefab(prefab);
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

        Unit prefab = unit.GetOriginPrefab();

        if (prefab == null)
        {
            Debug.LogError("originPrefab null임!");
            return;
        }

        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<Unit>();
        }

        pools[prefab].Enqueue(unit);
    }
}