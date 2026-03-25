using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 소환되는 유닛 리스트
/// </summary>
[CreateAssetMenu(fileName = "FriendlyUnitList", menuName = "Scriptable Objects/FriendlyUnitList")]
public class FriendlyUnitList : ScriptableObject
{
    [Header("Meadow")]
    [SerializeField] private List<FriendlyUnitData> meadowUnits = new List<FriendlyUnitData>();

    [Header("Ocean")]
    [SerializeField] private List<FriendlyUnitData> oceanUnits = new List<FriendlyUnitData>();

    [Header("Desert")]
    [SerializeField] private List<FriendlyUnitData> desertUnits = new List<FriendlyUnitData>();

    [Header("Forest")]
    [SerializeField] private List<FriendlyUnitData> forestUnits = new List<FriendlyUnitData>();

    [Header("Polar")]
    [SerializeField] private List<FriendlyUnitData> polarUnits = new List<FriendlyUnitData>();

    /// <summary>
    /// 외부 접근함수
    /// </summary>
    public List<FriendlyUnitData> GetUnits(Habitat habitat)
    {
        return habitat switch
        {
            Habitat.Meadow => meadowUnits,
            Habitat.Ocean => oceanUnits,
            Habitat.Desert => desertUnits,
            Habitat.Forest => forestUnits,
            Habitat.Polar => polarUnits,
            _ => null
        };
    }
}
