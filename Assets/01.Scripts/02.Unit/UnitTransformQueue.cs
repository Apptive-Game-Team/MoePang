using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 공격 우선순위 설정 큐
/// </summary>
public class UnitTransformQueue : MonoBehaviour
{
    public static UnitTransformQueue Instance { get; private set; }

    private Dictionary<TeamType, Queue<IDamageable>> teamQueues;
    private Dictionary<TeamType, IDamageable> teamCastles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        teamQueues = new Dictionary<TeamType, Queue<IDamageable>>()
        {   
            { TeamType.Friendly, new Queue<IDamageable>() },
            { TeamType.Enemy, new Queue<IDamageable>() }
        };

        teamCastles = new Dictionary<TeamType, IDamageable>();
    }

    public void Enqueue(TeamType team, IDamageable unit)
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

    public IDamageable Peek(TeamType team)
    {
        if (teamQueues[team].Count > 0)
            return teamQueues[team].Peek();

        if (teamCastles.ContainsKey(team))
            return teamCastles[team];

        return null;
    }

    public void Clear(TeamType team)
    {
        teamQueues[team].Clear();
    }
    public void RegisterCastle(TeamType team, IDamageable castle)
    {
        teamCastles[team] = castle;
    }


    /// <summary>
    /// 유닛 순서 큐 기즈모
    /// </summary>
    private void OnDrawGizmos()
    {
        if (teamQueues == null) return;

        foreach (var team in teamQueues.Keys)
        {
            IDamageable firstUnit = Peek(team);
            if (firstUnit != null)
            {
                Gizmos.color = (team == TeamType.Friendly) ? Color.blue : Color.red;

                Vector3 pos = firstUnit.GetTransform().position;
                Gizmos.DrawWireSphere(pos + Vector3.up * 1.5f, 0.3f);
            }
        }
    }

}
