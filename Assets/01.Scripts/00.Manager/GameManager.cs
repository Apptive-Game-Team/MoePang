using _01.Scripts._06.Shop;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts._00.Manager
{
    [Serializable]
    public class PlayData
    {
        public int goldAmount;
        public int clearedStage;
        public List<StageData> stagesData;

        public PlayData()
        {
            stagesData = new List<StageData>();
            for (int i = 0; i < 50; i++)
            {
                stagesData.Add(new StageData());
            }
        }
    }
    
    [Serializable]
    public class StageData
    {
        public int stageNum;
        public float minUsedTime;
        public int minUsedTile;
        public int maxUsedTile;
    }

    public interface IConvertable
    {
        public void BeforeSave();
        public void AfterLoad();
    }

    [Serializable]
    public class UnitData : IConvertable
    {
        public Dictionary<FriendlyUnitData, bool> UnlockedUnits = new();
        public Dictionary<FriendlyUnitData, int> UnitLevels = new();
        
        [SerializeField] private List<FriendlyUnitData> unitKeys;
        [SerializeField] private List<bool> unitValues;
        [SerializeField] private List<FriendlyUnitData> unitLevelKeys;
        [SerializeField] private List<int> unitLevelValues;

        public UnitData() { }

        public UnitData(FriendlyUnitList unitList)
        {
            UnlockedUnits.Clear();
            
            foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
            {
                List<FriendlyUnitData> units = unitList.GetUnits(habitat);
                if (units == null) continue;

                foreach (FriendlyUnitData unit in units)
                {
                    UnlockedUnits[unit] = false;
                    UnitLevels[unit] = Mathf.Max(1, Mathf.RoundToInt(unit.BaseUnitLevel));
                }

                if (units.Count > 0)
                {
                    UnlockedUnits[units[0]] = true;
                }
            }

            BeforeSave();
        }
        
        // Dict <-> List
        public void BeforeSave()
        {
            GameManager.DictionaryToLists(UnlockedUnits, out unitKeys, out unitValues);
            GameManager.DictionaryToLists(UnitLevels, out unitLevelKeys, out unitLevelValues);
        }

        public void AfterLoad()
        {
            UnlockedUnits = GameManager.ListsToDictionary(unitKeys, unitValues);
            UnitLevels = GameManager.ListsToDictionary(unitLevelKeys, unitLevelValues);

            foreach (var unit in UnlockedUnits.Keys)
            {
                if (!UnitLevels.ContainsKey(unit))
                {
                    UnitLevels[unit] = Mathf.Max(1, Mathf.RoundToInt(unit.BaseUnitLevel));
                }
            }
        }

        public int GetUnitLevel(FriendlyUnitData unit)
        {
            if (unit == null)
            {
                return 1;
            }

            if (!UnitLevels.TryGetValue(unit, out int level))
            {
                level = Mathf.Max(1, Mathf.RoundToInt(unit.BaseUnitLevel));
                UnitLevels[unit] = level;
            }

            return level;
        }

        public void IncreaseUnitLevel(FriendlyUnitData unit)
        {
            if (unit == null)
            {
                return;
            }

            UnitLevels[unit] = GetUnitLevel(unit) + 1;
        }
    }

    [Serializable]
    public class CastleData
    {
        public int castleLevel = 1;
    }

    [Serializable]
    public class ItemData : IConvertable
    {
        public Dictionary<ItemType, int> ItemAmounts = new();

        [SerializeField] private List<ItemType> items;
        [SerializeField] private List<int> amounts;

        public ItemData()
        {
            ItemAmounts.Clear();

            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                ItemAmounts.Add(itemType, 0);
            }
        }

        public void BeforeSave()
        {
            GameManager.DictionaryToLists(ItemAmounts, out items, out amounts);
        }

        public void AfterLoad()
        {
            ItemAmounts = GameManager.ListsToDictionary(items, amounts);
        }
    }
    
    [Serializable]
    public class ComboData : IConvertable
    {
        public List<Habitat> comboSequence = new()
        {
            Habitat.Meadow,
            Habitat.Ocean,
            Habitat.Desert,
            Habitat.Forest,
            Habitat.Polar
        };
        
        public Dictionary<Habitat, int> comboLevels = new();

        [SerializeField] private List<Habitat> combos;
        [SerializeField] private List<int> levels;

        public ComboData()
        {
            comboLevels.Clear();

            foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
            {
                comboLevels.Add(habitat, 1);
            }
        }
        
        public void BeforeSave()
        {
            GameManager.DictionaryToLists(comboLevels, out combos, out levels);
        }

        public void AfterLoad()
        {
            comboLevels = GameManager.ListsToDictionary(combos, levels);
        }
    }

    [Serializable]
    public class GameData
    {
        public SoundData soundData;
        public LanguageData languageData;
    }

    [Serializable]
    public class SoundData
    {
        public float masterVolume = 0.5f;
        public float bgmVolume = 0.5f;
        public float sfxVolume = 0.5f;
    }

    [Serializable]
    public class LanguageData
    {
        public int languageIndex;
    }
    
    public class GameManager : SingletonObject<GameManager>
    {
        [SerializeField] private FriendlyUnitList unitList;
        
        public PlayData playData;
        public CastleData castleData;
        public UnitData unitData;
        public ItemData itemData;
        public ComboData comboData;
        public GameData gameData;

        protected override void Awake()
        {
            base.Awake();
            
            playData = new PlayData();
            castleData = new CastleData();
            unitData = new UnitData(unitList);
            itemData = new ItemData();
            comboData = new ComboData();
            gameData = new GameData();
        }

        private void Start()
        {
            SaveLoadManager.Instance.LoadData(playData, "PlayData");
            SaveLoadManager.Instance.LoadData(castleData, "CastleData");
            SaveLoadManager.Instance.LoadData(unitData, "UnitData");
            SaveLoadManager.Instance.LoadData(itemData, "ItemData");
            SaveLoadManager.Instance.LoadData(comboData, "ComboData");
            SaveLoadManager.Instance.LoadData(gameData, "GameData");
        }
        
        public static void DictionaryToLists<TKey, TValue>(Dictionary<TKey, TValue> dict, out List<TKey> keys, out List<TValue> values)
        {
            keys = new List<TKey>();
            values = new List<TValue>();
    
            if (dict == null) return;

            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public static Dictionary<TKey, TValue> ListsToDictionary<TKey, TValue>(List<TKey> keys, List<TValue> values)
        {
            var dict = new Dictionary<TKey, TValue>();
            if (keys == null || values == null) return dict;

            for (int i = 0; i < keys.Count; i++)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }

        public void SavePlayData()
        {
            StageManager stageManager = StageManager.Instance;
            GoldManager goldManager = GoldManager.Instance;
            
            float time = stageManager.CurrentTime;
            int usedTileCount = stageManager.UsedTileCount;

            playData.goldAmount = goldManager.Gold;
            
            playData.clearedStage = Mathf.Max(playData.clearedStage, stageManager.CurrentStage + 1);
            stageManager.SetMaxStage(playData.clearedStage);

            StageData stageData = playData.stagesData[stageManager.CurrentStage];
            stageData.stageNum = stageManager.CurrentStage + 1;
            stageData.maxUsedTile = Mathf.Max(stageData.maxUsedTile, usedTileCount);
            stageData.minUsedTile = stageData.minUsedTile == 0 ?
                usedTileCount : Mathf.Min(stageData.maxUsedTile, usedTileCount);
            stageData.minUsedTime = stageData.minUsedTime == 0 ?
                time : Mathf.Min(stageData.minUsedTime, time);
            
            SaveLoadManager.Instance.SaveData(playData, "PlayData");
        }
        
        public void SaveCastleData()
        {
            SaveGoldData();
            
            SaveLoadManager.Instance.SaveData(castleData, "CastleData");
        }

        public void SaveUnitData()
        {
            SaveGoldData();
            
            unitData.BeforeSave();
            SaveLoadManager.Instance.SaveData(unitData, "UnitData");
        }
        
        public void SaveItemData()
        {
            SaveGoldData();
            
            itemData.BeforeSave();
            SaveLoadManager.Instance.SaveData(itemData, "ItemData");
        }

        public void SaveGameData()
        {
            SaveLoadManager.Instance.SaveData(gameData, "GameData");
        }

        public void SaveComboData()
        {
            SaveLoadManager.Instance.SaveData(comboData, "ComboData");
        }

        public void SaveGoldData()
        {
            playData.goldAmount = GoldManager.Instance.Gold;
            SaveLoadManager.Instance.SaveData(playData, "PlayData");
        }
    }
}
