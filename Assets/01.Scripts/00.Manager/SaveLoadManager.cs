using _01.Scripts._10.System;
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
        
        public void SaveData<T>(T data, string fileName)
        {
            string path = GetSavePath(fileName);
            
            if (data is IConvertable convert)
            {
                convert.BeforeSave();
            }
            
            string json = JsonUtility.ToJson(data, true);
            
            string encrypted = SaveDataEncryption.Encrypt(json);

            File.WriteAllText(path, encrypted);
            
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
                
                string decrypted = SaveDataEncryption.Decrypt(json);
                
                JsonUtility.FromJsonOverwrite(decrypted, data);
                
                if (data is IConvertable convert)
                {
                    convert.AfterLoad();
                }
            }
        }
    }
}