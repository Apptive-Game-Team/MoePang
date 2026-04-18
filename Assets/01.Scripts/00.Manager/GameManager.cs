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
        // todo : 해금된 기물 추가 예정

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
    public class UnitData
    {
        public Dictionary<UpgradeData, int> castleLevels = new();
        
        [SerializeField] private List<UpgradeData> castleKeys;
        [SerializeField] private List<int> castleValues;
        
        // Dict <-> List
        public void BeforeSave()
        {
            SaveLoadManager.DictionaryToLists(castleLevels, out castleKeys, out castleValues);
        }

        public void AfterLoad()
        {
            castleLevels = SaveLoadManager.ListsToDictionary(castleKeys, castleValues);
        }
    }

    [Serializable]
    public class StageData
    {
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
        public PlayData playData;
        public UnitData unitData;
        public GameData gameData;

        protected override void Awake()
        {
            base.Awake();
            
            playData = new PlayData();
            gameData = new GameData();
        }

        private void Start()
        {
            SaveLoadManager.Instance.LoadData(playData, "PlayData");
            SaveLoadManager.Instance.LoadData(unitData, "UnitData");
            SaveLoadManager.Instance.LoadData(gameData, "GameData");
        }

        public void SavePlayData()
        {
            StageManager stageManager = StageManager.Instance;
            GoldManager goldManager = GoldManager.Instance;
            
            float time = stageManager.CurrentTime;
            int usedTileCount = stageManager.UsedTileCount;

            playData.goldAmount = goldManager.Gold;
            
            playData.clearedStage = Mathf.Max(playData.clearedStage, stageManager.CurrentStage);
            stageManager.SetMaxStage(playData.clearedStage + 1);

            StageData stageData = playData.stagesData[stageManager.CurrentStage];
            stageData.maxUsedTile = Mathf.Max(stageData.maxUsedTile, usedTileCount);
            stageData.minUsedTile = stageData.minUsedTile == 0 ?
                usedTileCount : Mathf.Min(stageData.maxUsedTile, usedTileCount);
            stageData.minUsedTime = stageData.minUsedTime == 0 ?
                time : Mathf.Min(stageData.minUsedTime, time);
            
            SaveLoadManager.Instance.SaveData(playData, "PlayData");
        }
        
        public void SaveUnitData()
        {
            unitData.BeforeSave();
            
            SaveLoadManager.Instance.SaveData(unitData, "UnitData");
        }

        public void SaveGameData()
        {
            SaveLoadManager.Instance.SaveData(gameData, "GameData");
        }
    }
}
