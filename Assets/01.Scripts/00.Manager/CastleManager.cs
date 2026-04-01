using UnityEngine;

/// <summary>
/// Castle Data 관리 스크립트
/// </summary>
public class CastleManager : SingletonObject<CastleManager>
{
    [SerializeField] private float maxHp;

    public float MaxHp
    {
        get => maxHp; 
        set => maxHp = value;
    }
}