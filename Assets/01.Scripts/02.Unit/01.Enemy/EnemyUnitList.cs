using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 적 유닛 리스트
/// </summary>
[CreateAssetMenu(fileName = "EnemyUnitList", menuName = "Scriptable Objects/EnemyUnitList")]
public class EnemyUnitList : ScriptableObject
{
    [SerializeField] private List<EnemyUnitData> enemyUnits = new();

    public List<EnemyUnitData> GetUnits()
    {
        return enemyUnits;
    }
}