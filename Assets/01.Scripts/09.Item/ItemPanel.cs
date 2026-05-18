using _01.Scripts._00.Manager;
using _01.Scripts._01.ThreeMatch;
using _01.Scripts._06.Shop;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ItemData = _01.Scripts._06.Shop.ItemData;

namespace _01.Scripts._09.Item
{
    public class ItemPanel : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private PuzzleGenerator generator;

        private void Awake()
        {
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                GameObject item = Instantiate(itemPrefab, transform);
                item.GetComponent<Image>().sprite = itemData.items.First(info => info.type == type).sprite;
                item.GetComponentInChildren<TextMeshProUGUI>().text = GameManager.Instance.itemData.ItemAmounts[type].ToString();
                item.GetComponent<Item>().Init(type, generator);
            }
        }
    }
}
