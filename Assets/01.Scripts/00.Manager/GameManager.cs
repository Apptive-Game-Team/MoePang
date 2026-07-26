using _01.Scripts._06.Shop;
using System;
using System.Collections.Generic;
using UnityEngine;
using _01.Scripts._11.HabitatMode;
using _01.Scripts._08.Utility;
using UnityEngine.SceneManagement;

namespace _01.Scripts._00.Manager
{
    public interface IConvertable
    {
        public void BeforeSave();
        public void AfterLoad();
    }

    public enum StageType
    {
        Normal,
        Meadow,
        Ocean,
        Desert,
        Forest,
        Polar,
    }
    
    [Serializable]
    public class PlayData : IConvertable
    {
        public int goldAmount;
        public int diaAmount;
        
        public Dictionary<StageType, int> MaxStages = new();
        [SerializeField] private List<StageType> clearedStageKeys;
        [SerializeField] private List<int> clearedStageValues;

        public Dictionary<StageType, Dictionary<int, StageData>> StageData = new();
        [SerializeField] private List<StageType> stageTypeKeys;
        [SerializeField] private List<StageDataList> stageTypeValues;

        public List<int> selectedStage = new();

        public PlayData()
        {
            foreach (StageType type in Enum.GetValues(typeof(StageType)))
            {
                MaxStages[type] = 0;
                StageData[type] = new Dictionary<int, StageData>();
                selectedStage.Add(0);
            }
        }

        public void BeforeSave()
        {
            GameManager.DictionaryToLists(
                MaxStages,
                out clearedStageKeys,
                out clearedStageValues
            );
            
            stageTypeKeys = new List<StageType>();
            stageTypeValues = new List<StageDataList>();

            foreach (KeyValuePair<StageType, Dictionary<int, StageData>> pair in StageData)
            {
                stageTypeKeys.Add(pair.Key);

                stageTypeValues.Add(new StageDataList
                {
                    stages = new List<StageData>(pair.Value.Values)
                });
            }
        }

        public void AfterLoad()
        {
            MaxStages = GameManager.ListsToDictionary(
                clearedStageKeys,
                clearedStageValues
            );
            
            StageData = new Dictionary<StageType, Dictionary<int, StageData>>();

            for (int i = 0; i < stageTypeKeys.Count; i++)
            {
                Dictionary<int, StageData> stageDict = new();

                foreach (StageData stage in stageTypeValues[i].stages)
                {
                    stageDict[stage.stageNum - 1] = stage;
                }

                StageData[stageTypeKeys[i]] = stageDict;
            }
            
            foreach (StageType type in Enum.GetValues(typeof(StageType)))
            {
                MaxStages.TryAdd(type, 0);

                if (!StageData.ContainsKey(type))
                {
                    StageData[type] = new Dictionary<int, StageData>();
                }
            }
        }
    }

    [Serializable]
    public class StageDataList
    {
        public List<StageData> stages = new();
    }
    
    [Serializable]
    public class StageData
    {
        public int stageNum;
        public float minUsedTime;
        public int minUsedTile;
        public int maxUsedTile;
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
        
        public Dictionary<Habitat, int> ComboLevels = new();

        [SerializeField] private List<Habitat> combos;
        [SerializeField] private List<int> levels;

        public ComboData()
        {
            ComboLevels.Clear();

            foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
            {
                ComboLevels.Add(habitat, 1);
            }
        }
        
        public void BeforeSave()
        {
            GameManager.DictionaryToLists(ComboLevels, out combos, out levels);
        }

        public void AfterLoad()
        {
            ComboLevels = GameManager.ListsToDictionary(combos, levels);
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

            SceneManager.sceneLoaded += OnSceneLoaded;
            PlayBGMForScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayBGMForScene(scene.name);
        }

        private void PlayBGMForScene(string sceneName)
        {
            if (sceneName == SceneInfo.GetSceneName(SceneType.MatchAndBattle))
            {
                PlayBattleBGM();
                return;
            }

            if (sceneName == SceneInfo.GetSceneName(SceneType.HabitatModeSelect))
            {
                SoundManager.Instance.PlayHabitatModeSelectBGM();
                return;
            }

            SoundManager.Instance.PlayTitleAndLobbyBGM();
        }

        private void PlayBattleBGM()
        {
            if (HabitatModeManager.Instance != null &&
                HabitatModeManager.Instance.IsHabitatBattle)
            {
                SoundManager.Instance.PlayHabitatModeBGM();
                return;
            }

            if (StageManager.Instance.IsHighHurdleStage())
            {
                SoundManager.Instance.PlayHighHurdleStageBGM();
                return;
            }

            if (StageManager.Instance.IsMiddleHurdleStage())
            {
                SoundManager.Instance.PlayMiddleHurdleStageBGM();
                return;
            }

            SoundManager.Instance.PlayInGameDefaultBGM();
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
            playData.diaAmount = goldManager.Dia;

            StageType stageType;
            int currentStage;
            
            if (HabitatModeManager.Instance != null && HabitatModeManager.Instance.IsHabitatBattle)
            {
                HabitatMode mode = HabitatModeManager.Instance.HabitatMode;
                stageType = GetStageTypeWithHabitat(mode);
                playData.selectedStage[(int)stageType] = playData.MaxStages[stageType] == stageManager.CurrentHabitatStage ?
                    playData.MaxStages[stageType] + 1 : stageManager.CurrentStage;
                playData.MaxStages[stageType] = Mathf.Max(playData.MaxStages[stageType], stageManager.CurrentHabitatStage + 1);
                currentStage = stageManager.CurrentHabitatStage;
                stageManager.SetMaxHabitatStage(mode, Mathf.Max(stageManager.GetMaxHabitatStage(mode), stageManager.CurrentHabitatStage + 1));
            }
            else
            {
                stageType = StageType.Normal;
                playData.selectedStage[(int)stageType] = playData.MaxStages[stageType] == stageManager.CurrentStage ?
                    playData.MaxStages[stageType] + 1 : stageManager.CurrentStage;
                playData.MaxStages[stageType] = Mathf.Max(playData.MaxStages[stageType], stageManager.CurrentStage + 1);
                currentStage = stageManager.CurrentStage;
                stageManager.SetMaxStage(Mathf.Max(stageManager.MaxStage, stageManager.CurrentStage + 1));
            }
            
            if (!playData.StageData[stageType]
                    .TryGetValue(currentStage, out StageData stageData))
            {
                stageData = new StageData();
            }
            
            stageData.stageNum = currentStage + 1;
            stageData.maxUsedTile = Mathf.Max(stageData.maxUsedTile, usedTileCount);
            stageData.minUsedTile = stageData.minUsedTile == 0 ?
                usedTileCount : Mathf.Min(stageData.maxUsedTile, usedTileCount);
            stageData.minUsedTime = stageData.minUsedTime == 0 ?
                time : Mathf.Min(stageData.minUsedTime, time);
            
            playData.StageData[stageType][currentStage] = stageData;
            
            playData.BeforeSave();
            SaveLoadManager.Instance.SaveData(playData, "PlayData");
            
            if (StageManager.Instance != null)
            {
                StageManager.Instance.RefreshFromPlayData();
            }
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
            playData.diaAmount = GoldManager.Instance.Dia;
            
            playData.BeforeSave();
            SaveLoadManager.Instance.SaveData(playData, "PlayData");
        }

        public StageType GetStageTypeWithHabitat(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => StageType.Meadow,
                HabitatMode.OceanMode => StageType.Ocean,
                HabitatMode.DesertMode => StageType.Desert,
                HabitatMode.ForestMode => StageType.Forest,
                HabitatMode.PolarMode => StageType.Polar,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
        
        public int GetClearedHabitatStage(HabitatMode mode)
        {
            return mode switch
            {
                HabitatMode.MeadowMode => playData.MaxStages[StageType.Meadow],
                HabitatMode.OceanMode => playData.MaxStages[StageType.Ocean],
                HabitatMode.DesertMode => playData.MaxStages[StageType.Desert],
                HabitatMode.ForestMode => playData.MaxStages[StageType.Forest],
                HabitatMode.PolarMode => playData.MaxStages[StageType.Polar],
                _ => playData.MaxStages[StageType.Meadow]
            };
        }
    }
}
