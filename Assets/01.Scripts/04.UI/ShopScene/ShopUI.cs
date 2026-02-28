using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점에 Unit Icon 스크립트
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI 정보")]
    [SerializeField] private bool isUnlocked = false;
    [SerializeField] private int unitCost;
    [SerializeField] private Image unlockImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private bool isSelected = false;
    [SerializeField] private Habitat habitat;
    [SerializeField] private UnitType unitType;

    private ShopManager shopManager;

    public bool IsSelected => isSelected;
    public bool IsUnlocked => isUnlocked;
    public int UnitCost => unitCost;
    public Habitat Habitat => habitat;
    public UnitType UnitType => unitType;

    private void Start()
    {
        RefreshUnlockState();
    }

    public void RefreshUnlockState()
    {
        bool unlocked = HabitatManager.Instance.IsUnlocked(habitat, unitType);

        isUnlocked = unlocked;

        unlockImage.gameObject.SetActive(!unlocked);
    }

    public void SetManager(ShopManager manager)
    {
        shopManager = manager;
    }

    private void UnlockUnit()
    {
        unlockImage.sprite = null;
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
