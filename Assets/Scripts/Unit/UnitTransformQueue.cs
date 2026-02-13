using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 공격 우선순위 설정 큐
/// </summary>
public class UnitTransformQueue : MonoBehaviour
{
    public static UnitTransformQueue Instance { get; private set; }

    private Dictionary<TeamType, Queue<Unit>> teamQueues;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        teamQueues = new Dictionary<TeamType, Queue<Unit>>()
        {
            { TeamType.Friendly, new Queue<Unit>() },
            { TeamType.Enemy, new Queue<Unit>() }
        };
    }

    public void Enqueue(TeamType team, Unit unit)
    {
        teamQueues[team].Enqueue(unit);
    }

    public void Dequeue(TeamType team)
    {
        if (teamQueues[team].Count > 0)
            teamQueues[team].Dequeue();
    }

    public bool IsEmpty(TeamType team)
    {
        return teamQueues[team].Count == 0;

    }

    public Unit Peek(TeamType team)
    {
        if (teamQueues[team].Count > 0)
            return teamQueues[team].Peek();

        return null;
    }

    public void Clear(TeamType team)
    {
        teamQueues[team].Clear();
    }

    /// <summary>
    /// 유닛 순서 큐 기즈모
    /// </summary>
    private void OnDrawGizmos()
    {
        if (teamQueues == null) return;

        foreach (var team in teamQueues.Keys)
        {
            Unit firstUnit = Peek(team);
            if (firstUnit != null)
            {
                Gizmos.color = (team == TeamType.Friendly) ? Color.blue : Color.red;
                Gizmos.DrawWireSphere(firstUnit.transform.position + Vector3.up * 1.5f, 0.3f);
            }
        }
    }

}
