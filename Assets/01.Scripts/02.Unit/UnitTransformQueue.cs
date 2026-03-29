using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 공격 우선순위 설정 큐
/// </summary>
public class UnitTransformQueue : MonoBehaviour
{
    public static UnitTransformQueue Instance { get; private set; }

    private Dictionary<TeamType, Queue<Unit>> teamQueues;
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

        teamQueues = new Dictionary<TeamType, Queue<Unit>>()
        {   
            { TeamType.Friendly, new Queue<Unit>() },
            { TeamType.Enemy, new Queue<Unit>() }
        };

        teamCastles = new Dictionary<TeamType, IDamageable>();
    }

    /// <summary>
    /// 큐에 유닛 넣기
    /// </summary>
    public void Enqueue(TeamType team, Unit unit)
    {
        teamQueues[team].Enqueue(unit);
    }

    /// <summary>
    /// 큐에서 유닛/캐슬 빼기
    /// </summary>
    public void Dequeue(TeamType team)
    {
        if (teamQueues[team].Count > 0)
            teamQueues[team].Dequeue();
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
        //유닛이 있으면 유닛 반환
        if (teamQueues[team].Count > 0)
            return teamQueues[team].Peek();

        //유닛이 없으면 캐슬 반환
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
                Gizmos.color = (team == TeamType.Friendly) ? Color.blue : Color.red;

                Vector3 pos = firstUnit.GetTransform().position;
                Gizmos.DrawWireSphere(pos + Vector3.up * 1.5f, 0.3f);
            }
        }
    }
}
