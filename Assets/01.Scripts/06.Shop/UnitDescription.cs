using UnityEngine;
using TMPro;

public class UnitDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI hpText;

    private void Start()
    {
        FriendlyUnitData data = HabitatManager.Instance.SelectedUnitData;

        if (data == null)
        {
            return;
        }

        nameText.text = data.UnitName.ToString();
        descriptionText.text = data.UnitDescriptionText;
        damageText.text = data.AttackDamage.ToString();
        hpText.text = data.MaxHp.ToString();
    }
}
