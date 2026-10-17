using _01.Scripts._00.Manager;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestSpawn : MonoBehaviour
{
    [Header("Buttons (Hierarchy order)")]
    [SerializeField] private GridLayoutGroup buttonGrid;

    [Header("Spawn settings (same as UnitSpawner)")]
    [SerializeField] private FriendlyUnit friendlyPrefab;
    [SerializeField] private FriendlyUnitList friendlyUnitList;
    [SerializeField] private Transform friendlySpawnPosition;

    private const int UnitsPerHabitat = 5;
    private static readonly Habitat[] Habitats =
    {
        Habitat.Meadow, Habitat.Ocean, Habitat.Desert, Habitat.Forest, Habitat.Polar
    };

    private Button[] buttons;
    private UnityAction[] spawnActions;

    private void Awake()
    {
        if (buttonGrid == null)
            buttonGrid = GetComponent<GridLayoutGroup>();

        if (buttonGrid == null)
        {
            Debug.LogError("TestSpawn: Assign the button Grid Layout Group.", this);
            return;
        }

        buttons = buttonGrid.GetComponentsInChildren<Button>(true);
        int count = Mathf.Min(buttons.Length, Habitats.Length * UnitsPerHabitat);
        spawnActions = new UnityAction[count];

        if (buttons.Length != Habitats.Length * UnitsPerHabitat)
            Debug.LogWarning($"TestSpawn: Expected 25 buttons, found {buttons.Length}.", this);

        for (int i = 0; i < count; i++)
        {
            int buttonIndex = i;
            spawnActions[i] = () => SpawnUnit(buttonIndex);
            buttons[i].onClick.AddListener(spawnActions[i]);
        }
    }

    private void Start()
    {
        if (!GameManager.Instance.isTestMode)
        {
            Destroy(gameObject);
        }
    }

    private void SpawnUnit(int buttonIndex)
    {
        if (friendlyPrefab == null || friendlyUnitList == null ||
            friendlySpawnPosition == null || UnitPool.Instance == null)
        {
            Debug.LogError("TestSpawn: Check prefab, unit list, spawn position, and UnitPool.", this);
            return;
        }

        Habitat habitat = Habitats[buttonIndex / UnitsPerHabitat];
        int unitIndex = buttonIndex % UnitsPerHabitat;
        var units = friendlyUnitList.GetUnits(habitat);

        if (units == null || unitIndex >= units.Count || units[unitIndex] == null)
        {
            Debug.LogWarning($"TestSpawn: Missing {habitat} unit at index {unitIndex}.", this);
            return;
        }

        UnitPool.Instance.Get(friendlyPrefab, units[unitIndex], friendlySpawnPosition);
    }

    private void OnDestroy()
    {
        if (spawnActions == null) return;

        for (int i = 0; i < spawnActions.Length; i++)
        {
            if (buttons[i] != null)
                buttons[i].onClick.RemoveListener(spawnActions[i]);
        }
    }
}
