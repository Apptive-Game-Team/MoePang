using System.Collections.Generic;
using UnityEngine;

public static class UnitSortingManager
{
    private const int FirstOrder = 1000;
    // Leave space below the common hit effects (30000+) and for unit VFX (+10).
    private const int LastOrder = 29988;
    private const int OrderStep = 2;
    private static readonly List<SpriteRenderer> activeRenderers = new List<SpriteRenderer>();
    private static int nextOrder = FirstOrder;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        activeRenderers.Clear();
        nextOrder = FirstOrder;
    }

    public static void Register(SpriteRenderer renderer)
    {
        if (renderer == null) return;

        // A pooled unit receives a new place at the front on every spawn.
        activeRenderers.Remove(renderer);

        if (nextOrder > LastOrder)
        {
            // Reclaim unused orders without changing the surviving units' relative order.
            activeRenderers.RemoveAll(item => item == null);
            nextOrder = FirstOrder;
            foreach (SpriteRenderer item in activeRenderers)
            {
                item.sortingOrder = nextOrder;
                nextOrder += OrderStep;
            }
        }

        if (nextOrder > LastOrder)
            throw new System.InvalidOperationException("Too many active units for unique sorting orders.");

        renderer.sortingOrder = nextOrder;
        nextOrder += OrderStep;
        activeRenderers.Add(renderer);
    }

    public static void Unregister(SpriteRenderer renderer)
    {
        activeRenderers.Remove(renderer);
    }
}
