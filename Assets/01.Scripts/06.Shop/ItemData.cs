using System;
using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts._06.Shop
{
    public enum ItemType
    {
        Joker,
        DestroyObstacle,
        CreateLineBomb,
        RaiseSpawnProb,
    }

    [Serializable]
    public class ItemInfo
    {
        public ItemType type;
        public int price;
        public Sprite sprite;
        [TextArea] public string itemDescription;
    }
    
    [CreateAssetMenu(menuName = "Shop/Item Data")]
    public class ItemData : ScriptableObject
    {
        public List<ItemInfo> items;
    }
}
