using UnityEngine;
using System.Collections.Generic;

public class UnitTransfromQueue : MonoBehaviour
{
    public static UnitTransfromQueue Instance { get; private set; }

    public Queue<Unit> queue = new Queue<Unit>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
