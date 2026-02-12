using UnityEngine;
using System.Collections.Generic;

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
}
