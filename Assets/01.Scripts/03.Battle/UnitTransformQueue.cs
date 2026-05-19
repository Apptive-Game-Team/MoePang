using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공격 우선순위 설정 큐
/// </summary>
public class UnitTransformQueue : MonoBehaviour
{
    public static UnitTransformQueue Instance { get; private set; }

    private Dictionary<TeamType, LinkedList<Unit>> teamQueues;
    private Dictionary<TeamType, IDamageable> teamCastles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        teamQueues = new Dictionary<TeamType, LinkedList<Unit>>()
        {
            { TeamType.Friendly, new LinkedList<Unit>() },
            { TeamType.Enemy, new LinkedList<Unit>() }
        };

        teamCastles = new Dictionary<TeamType, IDamageable>();
    }

    /// <summary>
    /// 큐에 유닛 넣기
    /// </summary>
    public void Enqueue(TeamType team, Unit unit)
    {
        if (teamQueues[team].Contains(unit))
            return;

        teamQueues[team].AddLast(unit);
    }

    // 특정 유닛이 죽었을 때 호출하여 큐에서 제거하는 기능 추가
    public void RemoveUnit(TeamType team, Unit unit)
    {
        teamQueues[team].Remove(unit);
    }

    /// <summary>
    /// 큐에서 유닛/캐슬 빼기
    /// </summary>
    public void Dequeue(TeamType team)
    {
        if (teamQueues[team].Count > 0)
            teamQueues[team].RemoveFirst();
    }

    /// <summary>
    /// 큐가 비어있는지 확인
    /// </summary>
    public bool IsEmpty(TeamType team)
    {
        return teamQueues[team].Count == 0;
    }

    /// <summary>
    /// 큐 맨 앞의 유닛/캐슬 확인
    /// </summary>
    public IDamageable Peek(TeamType team)
    {
        if (teamQueues[team].Count > 0)
            return teamQueues[team].First.Value;

        if (teamCastles.ContainsKey(team))
            return teamCastles[team];

        return null;
    }

    /// <summary>
    /// 큐 초기화
    /// </summary>
    public void Clear(TeamType team)
    {
        teamQueues[team].Clear();
    }

    /// <summary>
    /// 각 팀의 캐슬 등록
    /// </summary>
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
                Gizmos.color = team == TeamType.Friendly ? Color.blue : Color.red;

                Vector3 pos = firstUnit.GetTransform().position;
                Gizmos.DrawWireSphere(pos + Vector3.up * 1.5f, 0.3f);
            }
        }
    }
}
