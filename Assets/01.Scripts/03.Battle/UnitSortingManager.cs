using UnityEngine;

public static class UnitSortingManager
{
    private static int currentOrder = 2;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        currentOrder = 2;
        Debug.Log("UnitSortingManager: 씬 로드로 인해 순서가 초기화되었습니다.");
    }

    public static int GetNextOrder()
    {
        currentOrder++;
        return currentOrder;
    }
}