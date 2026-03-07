using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct UnitListComponent
{
    [SerializeField] private Habitat habitat;
    [SerializeField] private UnitType unitType;
    [SerializeField] private UnitInFo unitInFo;
    [SerializeField] private int unlockValue;

    //프로퍼티
    public Habitat Habitat => habitat;
    public UnitType UnitType => unitType;
    public UnitInFo UnitInFo => unitInFo;
    public int UnlockValue => unlockValue;
}

/// <summary>
/// 소환되는 유닛 리스트
/// </summary>
[CreateAssetMenu(fileName = "UnitList", menuName = "Scriptable Objects/UnitList")]
public class UnitList : ScriptableObject
{
    [Header("Meadow")]
    [SerializeField] List<UnitListComponent> meadowList = new List<UnitListComponent>();

    [Header("Aqua")]
    [SerializeField] List<UnitListComponent> aquaList = new List<UnitListComponent>();
}
