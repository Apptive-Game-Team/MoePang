using System;
using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts._00.Manager
{
    [Serializable]
    public class PlayerData
    {
        public int goldAmount;
        public int clearedStage = -1;
        public List<StageData> stagesData;
        // todo : 해금된 기물, 강화 요소 별 강화 수치 추가 예정

        public PlayerData()
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
        public PlayerData playerData;
        public GameData gameData;

        protected override void Awake()
        {
            base.Awake();
            
            playerData = new PlayerData();
            gameData = new GameData();
        }

        private void Start()
        {
            SaveLoadManager.Instance.LoadData(playerData, "PlayerData");
            SaveLoadManager.Instance.LoadData(gameData, "GameData");
        }

        public void SavePlayerData()
        {
            SaveLoadManager.Instance.SaveData(playerData, "PlayerData");
        }

        public void SaveGameData()
        {
            SaveLoadManager.Instance.SaveData(gameData, "GameData");
        }
    }
}
