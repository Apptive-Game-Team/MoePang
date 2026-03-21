using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 소환되는 유닛 리스트
/// </summary>
[CreateAssetMenu(fileName = "UnitList", menuName = "Scriptable Objects/UnitList")]
public class UnitList : ScriptableObject
{
    [Header("Meadow")]
    [SerializeField] private List<UnitData> meadowUnits = new List<UnitData>();

    [Header("Ocean")]
    [SerializeField] private List<UnitData> oceanUnits = new List<UnitData>();

    [Header("Desert")]
    [SerializeField] private List<UnitData> desertUnits = new List<UnitData>();

    [Header("Forest")]
    [SerializeField] private List<UnitData> forestUnits = new List<UnitData>();

    [Header("Polar")]
    [SerializeField] private List<UnitData> polarUnits = new List<UnitData>();

    /// <summary>
    /// 외부 접근함수
    /// </summary>
    public List<UnitData> GetUnits(Habitat habitat)
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
