using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UnitInfoScene의 Unit Icon Script
/// </summary>
public class UnitInfoIcon : MonoBehaviour
{
    [Header("Unit Data")]
    [SerializeField] private FriendlyUnitData unitData;

    [Header("UI 정보")]
    [SerializeField] private Image unlockImage;
    [SerializeField] private TextMeshProUGUI levelImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float selectedScale = 1.08f;

    //참조
    private ShopManager shopManager;
    private bool isSelected = false;
    private Vector3 unlockImageOriginScale;
    private Vector3 backgroundImageOriginScale;

    //프로퍼티
    public FriendlyUnitData UnitData => unitData;
    public bool IsUnlocked => HabitatManager.Instance.IsUnlocked(unitData);

    private void Awake()
    {
        if (unlockImage != null)
        {
            unlockImageOriginScale = unlockImage.transform.localScale;
        }

        if (backgroundImage != null)
        {
            backgroundImageOriginScale = backgroundImage.transform.localScale;
        }
    }

    private void Start()
    {
        RefreshUnlockState();
    }

    /// <summary>
    /// ShopManager에서 Script 연결
    /// </summary>
    public void SetManager(ShopManager manager)
    {
        shopManager = manager;
    }
    
    public void Select()
    {
        isSelected = true;
        SetImageScale(selectedScale);
    }

    public void Deselect()
    {
        isSelected = false;
        SetImageScale(1f);
    }
    
    private void SetImageScale(float scale)
    {
        if (unlockImage != null)
        {
            unlockImage.transform.localScale = unlockImageOriginScale * scale;
        }

        if (backgroundImage != null)
        {
            backgroundImage.transform.localScale = backgroundImageOriginScale * scale;
        }
    }

    /// <summary>
    /// 씬 갱신시 Image Update
    /// </summary>
    public void RefreshUnlockState()
    {
        bool unlocked = HabitatManager.Instance.IsUnlocked(unitData);

        unlockImage.gameObject.SetActive(!unlocked);
        levelImage.gameObject.SetActive(unlocked);
        backgroundImage.gameObject.SetActive(unlocked);
        
        levelImage.text = $"Lv.{unitData.UnitLevel}";
    }

    public void OnClick()
    {
        if (!IsUnlocked && HabitatManager.Instance.CanUnlock(unitData))
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            shopManager.UnlockUnit(unitData, RefreshUnlockState);
            return;
        }
        
        if (!HabitatManager.Instance.CanUnlock(unitData))
        {
            SoundManager.Instance.PlaySFX(SFX.SFX1_ButtonClick);
            return;
        }
        
        shopManager.OnClickUnit(this);
    }
}
