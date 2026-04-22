using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _01.Scripts._00.Manager
{
    [Serializable]
    public class PlayData
    {
        public int goldAmount;
        public int clearedStage = -1;
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

    public interface IConvertable
    {
        public void BeforeSave();
        public void AfterLoad();
    }

    [Serializable]
    public class UnitData : IConvertable
    {
        public Dictionary<FriendlyUnitData, bool> unlockedUnits = new();
        
        [SerializeField] private List<FriendlyUnitData> unitKeys;
        [SerializeField] private List<bool> unitValues;

        public UnitData() { }

        public UnitData(FriendlyUnitList unitList)
        {
            unlockedUnits.Clear();
            
            foreach (Habitat habitat in Enum.GetValues(typeof(Habitat)))
            {
                List<FriendlyUnitData> units = unitList.GetUnits(habitat);
                if (units == null) continue;

                foreach (FriendlyUnitData unit in units)
                {
                    unlockedUnits[unit] = false;
                }

                if (units.Count > 0)
                {
                    unlockedUnits[units[0]] = true;
                }
            }

            BeforeSave();
        }
        
        // Dict <-> List
        public void BeforeSave()
        {
            GameManager.DictionaryToLists(unlockedUnits, out unitKeys, out unitValues);
        }

        public void AfterLoad()
        {
            unlockedUnits = GameManager.ListsToDictionary(unitKeys, unitValues);
        }
    }

    [Serializable]
    public class CastleData : IConvertable
    {
        public Dictionary<UpgradeData, int> castleLevels = new();
        
        [SerializeField] private List<UpgradeData> castleKeys;
        [SerializeField] private List<int> castleValues;
        
        // Dict <-> List
        public void BeforeSave()
        {
            GameManager.DictionaryToLists(castleLevels, out castleKeys, out castleValues);
        }

        public void AfterLoad()
        {
            castleLevels = GameManager.ListsToDictionary(castleKeys, castleValues);
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
        public GameData gameData;

        protected override void Awake()
        {
            base.Awake();
            
            playData = new PlayData();
            castleData = new CastleData();
            unitData = new UnitData(unitList);
            gameData = new GameData();
        }

        private void Start()
        {
            SaveLoadManager.Instance.LoadData(playData, "PlayData");
            SaveLoadManager.Instance.LoadData(castleData, "CastleData");
            SaveLoadManager.Instance.LoadData(unitData, "UnitData");
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
            playData.goldAmount = GoldManager.Instance.Gold;
            castleData.BeforeSave();
            
            SaveLoadManager.Instance.SaveData(playData, "PlayData");
            SaveLoadManager.Instance.SaveData(castleData, "CastleData");
        }

        public void SaveUnitData()
        {
            playData.goldAmount = GoldManager.Instance.Gold;
            unitData.BeforeSave();
            
            SaveLoadManager.Instance.SaveData(playData, "PlayData");
            SaveLoadManager.Instance.SaveData(unitData, "UnitData");
        }

        public void SaveGameData()
        {
            SaveLoadManager.Instance.SaveData(gameData, "GameData");
        }
    }
}
