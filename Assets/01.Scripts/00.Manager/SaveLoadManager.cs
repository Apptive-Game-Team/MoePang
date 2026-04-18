using System.Collections.Generic;
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
                
                if (data is UnitData unitData)
                {
                    unitData.AfterLoad();
                }
            }
        }
    }
}