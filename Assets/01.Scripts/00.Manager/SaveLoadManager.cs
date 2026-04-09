using System.IO;
using UnityEngine;

namespace _01.Scripts._00.Manager
{
    public class SaveLoadManager : SingletonObject<SaveLoadManager>
    {
        private string GetSavePath(string fileName)
        {
            #if UNITY_EDITOR
                return Path.Combine(Application.dataPath, $"SaveData/{fileName}.json");
            #else
                return Path.Combine(Application.persistentDataPath, $"{fileName}Save.json");
            #endif
        }
        
        public void SaveData<T>(T data, string fileName)
        {
            string path = GetSavePath(fileName);
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(path, json);
            
            #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
            #endif
        }
        
        public void LoadData<T>(T data, string fileName)
        {
            string path = GetSavePath(fileName);
            
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, data);
            }
        }
    }
}