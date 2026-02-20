using UnityEngine;

public class GoldManager : SingletonObject<GoldManager>
{
    [Header("소지금")]
    [SerializeField] private float gold;

    public float Gold => gold;
}
