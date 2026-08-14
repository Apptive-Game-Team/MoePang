using System.Collections.Generic;
using UnityEngine;

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

    private readonly Dictionary<Unit, long> unitOrders = new();
    private Dictionary<TeamType, LinkedList<QueueEntry>> teamLists;
    private Dictionary<TeamType, IDamageable> teamCastles;
    private long nextOrder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        teamLists = new Dictionary<TeamType, LinkedList<QueueEntry>>()
        {
            { TeamType.Friendly, new LinkedList<QueueEntry>() },
            { TeamType.Enemy, new LinkedList<QueueEntry>() }
        };

        teamCastles = new Dictionary<TeamType, IDamageable>();
    }

    public void ResetAndInsert(Unit unit)
    {
        Remove(unit, true);
        Insert(unit);
    }

    public void Insert(Unit unit)
    {
        if (unit == null) return;
        if (!unit.gameObject.activeInHierarchy) return;
        if (unit.CurrentHp <= 0f) return;

        Remove(unit, false);

        if (!unitOrders.ContainsKey(unit))
            unitOrders[unit] = nextOrder++;

        TeamType team = unit.GetTeam();
        QueueEntry entry = new QueueEntry(unit, unitOrders[unit]);
        InsertSorted(team, entry);
    }

    public void Enqueue(TeamType team, Unit unit)
    {
        Insert(unit);
    }

    public void RefreshUnit(TeamType team, Unit unit)
    {
        Insert(unit);
    }

    public void Remove(Unit unit)
    {
        Remove(unit, false);
    }

    public void RemoveUnit(TeamType team, Unit unit)
    {
        Remove(unit, true);
    }

    public void Dequeue(TeamType team)
    {
        if (teamLists[team].Count <= 0) return;

        Unit unit = teamLists[team].First.Value.Unit;
        unitOrders.Remove(unit);
        teamLists[team].RemoveFirst();
    }

    public bool IsEmpty(TeamType team)
    {
        RemoveInvalidUnits(team);
        return teamLists[team].Count == 0;
    }

    public IDamageable Peek(TeamType team)
    {
        RemoveInvalidUnits(team);

        Unit frontUnit = null;

        if (teamLists[team].Count > 0)
            frontUnit = teamLists[team].First.Value.Unit;

        teamCastles.TryGetValue(team, out IDamageable castle);

        if (frontUnit == null)
            return castle;

        if (castle == null)
            return frontUnit;

        float unitX = frontUnit.transform.position.x;
        float castleX = castle.GetTransform().position.x;

        bool castleIsFront =
            team == TeamType.Friendly
                ? castleX > unitX
                : castleX < unitX;

        return castleIsFront ? castle : frontUnit;
    }

    public List<IDamageable> PeekTargets(TeamType team, int count)
    {
        RemoveInvalidUnits(team);

        List<IDamageable> targets = new();

        foreach (QueueEntry entry in teamLists[team])
            targets.Add(entry.Unit);

        if (teamCastles.TryGetValue(team, out IDamageable castle))
            targets.Add(castle);

        targets.Sort((a, b) =>
        {
            float ax = a.GetTransform().position.x;
            float bx = b.GetTransform().position.x;

            if (team == TeamType.Friendly)
                return bx.CompareTo(ax);

            return ax.CompareTo(bx);
        });

        if (targets.Count > count)
            targets.RemoveRange(count, targets.Count - count);

        return targets;
    }

    public bool HasMovedBehindAnotherUnit(Unit unit)
    {
        if (unit == null) return false;

        TeamType team = unit.GetTeam();
        float unitX = unit.transform.position.x;

        foreach (QueueEntry entry in teamLists[team])
        {
            Unit other = entry.Unit;

            if (other == null || other == unit) continue;

            float otherX = other.transform.position.x;

            if (team == TeamType.Friendly && unitX < otherX)
                return true;

            if (team == TeamType.Enemy && unitX > otherX)
                return true;
        }

        return false;
    }

    public void Clear(TeamType team)
    {
        foreach (QueueEntry entry in teamLists[team])
            unitOrders.Remove(entry.Unit);

        teamLists[team].Clear();
    }

    public void RegisterCastle(TeamType team, IDamageable castle)
    {
        teamCastles[team] = castle;
    }

    private void Remove(Unit unit, bool removeOrder)
    {
        if (unit == null) return;

        foreach (LinkedList<QueueEntry> list in teamLists.Values)
        {
            for (LinkedListNode<QueueEntry> node = list.First; node != null; node = node.Next)
            {
                if (node.Value.Unit != unit) continue;

                list.Remove(node);

                if (removeOrder)
                    unitOrders.Remove(unit);

                return;
            }
        }

        if (removeOrder)
            unitOrders.Remove(unit);
    }

    private void InsertSorted(TeamType team, QueueEntry newEntry)
    {
        LinkedList<QueueEntry> list = teamLists[team];

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
        LinkedList<QueueEntry> list = teamLists[team];
        LinkedListNode<QueueEntry> node = list.First;

        while (node != null)
        {
            LinkedListNode<QueueEntry> next = node.Next;
            Unit unit = node.Value.Unit;

            if (unit == null || !unit.gameObject.activeInHierarchy || unit.CurrentHp <= 0f)
            {
                if (unit != null)
                    unitOrders.Remove(unit);

                list.Remove(node);
            }

            node = next;
        }
    }
    
    public Unit PeekFrontUnit(TeamType team)
    {
        RemoveInvalidUnits(team);

        if (teamLists[team].Count <= 0)
            return null;

        return teamLists[team].First.Value.Unit;
    }

    public IDamageable PeekCastle(TeamType team)
    {
        teamCastles.TryGetValue(team, out IDamageable castle);
        return castle;
    }
    
    public Unit PeekFrontUnitForCamera(TeamType team)
    {
        RemoveInvalidUnits(team);

        Unit frontUnit = null;

        foreach (Unit unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (unit == null) continue;
            if (!unit.gameObject.activeInHierarchy) continue;
            if (unit.CurrentHp <= 0f) continue;
            if (unit.GetTeam() != team) continue;

            Insert(unit);

            if (frontUnit == null)
            {
                frontUnit = unit;
                continue;
            }

            float unitX = unit.transform.position.x;
            float frontX = frontUnit.transform.position.x;

            if (team == TeamType.Friendly)
            {
                if (unitX > frontX)
                    frontUnit = unit;
            }
            else
            {
                if (unitX < frontX)
                    frontUnit = unit;
            }
        }

        return frontUnit;
    }

    private void OnDrawGizmos()
    {
        if (teamLists == null) return;

        foreach (TeamType team in teamLists.Keys)
        {
            IDamageable firstUnit = Peek(team);
            if (firstUnit == null) continue;

            Gizmos.color = team == TeamType.Friendly ? Color.blue : Color.red;

            Vector3 pos = firstUnit.GetTransform().position;
            Gizmos.DrawWireSphere(pos + Vector3.up * 1.5f, 0.3f);
        }
    }
}
