using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private UpgradeData data;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI levelText;

    private ShopManager shopManager;
    private bool isSelected;

    public UpgradeData Data => data;

    void Start()
    {
        Refresh();
    }

    public void SetManager(ShopManager manager)
    {
        shopManager = manager;
    }

    public void OnClick()
    {
        shopManager.OnClickUpgrade(this);
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

    public void Refresh()
    {
        int level = UpgradeManager.Instance.GetLevel(data);

        levelText.text = $"{data.UpgradeType} Lv.{level}/{data.MaxLevel}";
    }
}