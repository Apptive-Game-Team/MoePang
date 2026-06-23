using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점에 Unit Icon 스크립트
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Unit Data")]
    [SerializeField] private FriendlyUnitData unitData;

    [Header("UI 정보")]
    [SerializeField] private Image unlockImage;
    [SerializeField] private TextMeshProUGUI levelImage;
    [SerializeField] private Image backgroundImage;

    //참조
    private ShopManager shopManager;
    private bool isSelected = false;

    //프로퍼티
    public FriendlyUnitData UnitData => unitData;
    public bool IsSelected => isSelected;
    public bool IsUnlocked => HabitatManager.Instance.IsUnlocked(unitData);
    public float UnitCost => unitData.UnitCost;

    private void Start()
    {
        RefreshUnlockState();
    }

    public void SetManager(ShopManager manager)
    {
        shopManager = manager;
    }

    /// <summary>
    /// 해금상태 UI 갱신
    /// </summary>
    public void RefreshUnlockState()
    {
        bool unlocked = HabitatManager.Instance.IsUnlocked(unitData);

        unlockImage.gameObject.SetActive(!unlocked);
        levelImage.gameObject.SetActive(unlocked);
        backgroundImage.gameObject.SetActive(unlocked);
        
        SetLevelText();
    }

    private void SetLevelText()
    {
        levelImage.text = $"Lv.{unitData.UnitLevel}";
    }

    public void OnClick()
    {
        shopManager.OnClickUnit(this);
    }

    public void Select()
    {
        isSelected = true;
        backgroundImage.color = Color.black;
    }

    public void Deselect()
    {
        isSelected = false;
        backgroundImage.color = Color.white;
    }
}
