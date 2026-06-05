using TMPro;
using UnityEngine;

namespace _01.Scripts._06.Shop
{
    public class ShopCanvas : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI gold;

        private void Awake()
        {
            OnGoldChanged();
        }
        
        private void OnEnable()
        {
            GoldManager.Instance.OnGoldChanged += OnGoldChanged;
        }

        private void OnDisable()
        {
            GoldManager.Instance.OnGoldChanged -= OnGoldChanged;
        }
        
        private void OnGoldChanged()
        {
            gold.text = $"{GoldManager.Instance.Gold}";
        }
    }
}
