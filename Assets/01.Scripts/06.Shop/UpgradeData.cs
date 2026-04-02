using UnityEngine;

public enum UpgradeType
{
    CastleHP,
    CastleAttack,
    GoldGain
}

[CreateAssetMenu(menuName = "Shop/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("업그레이드 데이터 세팅")]
    [SerializeField] private UpgradeType upgradeType;

    [SerializeField] private int maxLevel = 10;
    [SerializeField] private int increasePerLevel = 10;
    [SerializeField] private int baseCost = 100;

    //프로퍼티
    public UpgradeType UpgradeType => upgradeType;
    public int MaxLevel => maxLevel;
    public int IncreasePerLevel => increasePerLevel;
    public int BaseCost => baseCost;
}