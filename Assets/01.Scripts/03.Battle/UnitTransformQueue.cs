using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공격 우선순위 설정 큐
/// </summary>
public class UnitTransformQueue : MonoBehaviour
{
    public static UnitTransformQueue Instance { get; private set; }
    
    private class QueueEntry
    {
        public Unit Unit;
        public long Order;

        public QueueEntry(Unit unit, long order)
        {
            Unit = unit;
            Order = order;
        }
    }

    private Dictionary<TeamType, LinkedList<QueueEntry>> teamQueues;
    private Dictionary<TeamType, IDamageable> teamCastles;
    
    private Dictionary<Unit, long> unitOrders = new();
    private long nextOrder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        teamQueues = new Dictionary<TeamType, LinkedList<QueueEntry>>()
        {
            { TeamType.Friendly, new LinkedList<QueueEntry>() },
            { TeamType.Enemy, new LinkedList<QueueEntry>() }
        };

        teamCastles = new Dictionary<TeamType, IDamageable>();
    }

    /// <summary>
    /// 큐에 유닛 넣기
    /// </summary>
    public void Enqueue(TeamType team, Unit unit)
    {
        if (unit == null) return;

        RemoveUnit(team, unit, false);

        if (!unitOrders.ContainsKey(unit))
            unitOrders[unit] = nextOrder++;

        QueueEntry entry = new QueueEntry(unit, unitOrders[unit]);
        InsertSorted(team, entry);
    }
    
    public void RefreshUnit(TeamType team, Unit unit)
    {
        Enqueue(team, unit);
    }
    
    // 특정 유닛이 죽었을 때 호출하여 큐에서 제거하는 기능 추가
    public void RemoveUnit(TeamType team, Unit unit)
    {
        RemoveUnit(team, unit, true);
    }
    
    private void RemoveUnit(TeamType team, Unit unit, bool removeOrder)
    {
        LinkedList<QueueEntry> list = teamQueues[team];

        for (LinkedListNode<QueueEntry> node = list.First; node != null; node = node.Next)
        {
            if (node.Value.Unit == unit)
            {
                list.Remove(node);
                break;
            }
        }

        if (removeOrder)
            unitOrders.Remove(unit);
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
        RemoveInvalidUnits(team);

        if (teamQueues[team].Count > 0)
            return teamQueues[team].First.Value.Unit;

        if (teamCastles.ContainsKey(team))
            return teamCastles[team];

        return null;
    }

    /// <summary>
    /// 큐 초기화
    /// </summary>
    public void Clear(TeamType team)
    {
        foreach (QueueEntry entry in teamQueues[team])
            unitOrders.Remove(entry.Unit);

        teamQueues[team].Clear();
    }

    /// <summary>
    /// 각 팀의 캐슬 등록
    /// </summary>
    public void RegisterCastle(TeamType team, IDamageable castle)
    {
        teamCastles[team] = castle;
    }
    
    private void InsertSorted(TeamType team, QueueEntry newEntry)
    {
        LinkedList<QueueEntry> list = teamQueues[team];

        if (list.Count == 0)
        {
            list.AddFirst(newEntry);
            return;
        }

        for (LinkedListNode<QueueEntry> node = list.First; node != null; node = node.Next)
        {
            if (ShouldBeBefore(team, newEntry, node.Value))
            {
                list.AddBefore(node, newEntry);
                return;
            }
        }

        list.AddLast(newEntry);
    }

    private bool ShouldBeBefore(TeamType team, QueueEntry a, QueueEntry b)
    {
        float ax = a.Unit.transform.position.x;
        float bx = b.Unit.transform.position.x;

        if (!Mathf.Approximately(ax, bx))
        {
            if (team == TeamType.Friendly)
                return ax > bx;

            return ax < bx;
        }

        return a.Order < b.Order;
    }

    private void RemoveInvalidUnits(TeamType team)
    {
        LinkedList<QueueEntry> list = teamQueues[team];
        LinkedListNode<QueueEntry> node = list.First;

        while (node != null)
        {
            LinkedListNode<QueueEntry> next = node.Next;

            if (node.Value.Unit == null || !node.Value.Unit.gameObject.activeInHierarchy)
            {
                unitOrders.Remove(node.Value.Unit);
                list.Remove(node);
            }

            node = next;
        }
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
