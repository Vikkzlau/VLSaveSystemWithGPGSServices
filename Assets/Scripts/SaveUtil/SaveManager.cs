using System;
using UnityEngine;

namespace VLSaveSystemWithGPGSServices
{
    public class SaveManager<T>
    {
        public readonly string savePath;
        private readonly string saveKey = "temp";

        public SaveManager(string savefileName, string saveKey)
        {
            this.saveKey = saveKey;
            savePath = Application.persistentDataPath + savefileName;
            GPGSSaveLoadUtil.InitializeAssets();
        }

        public void Save(object saveData)
        {
            GPGSSaveLoadUtil.SaveObject(saveData, savePath, saveKey, typeof(T));
        }

        public T Load(out SaveLoadMethod saveLoadMethod)
        {
            object loadedObj = GPGSSaveLoadUtil.LoadObject(savePath, saveKey, typeof(T), out saveLoadMethod);

            // using try catch here, since GPGS loading may have problems.
            try
            {
                T loadData = (T)loadedObj;
                // Data loaded successfully.
                return loadData;
            }
            catch (Exception e)
            {
                Debug.Log("Object type cast failed during loading. " + e.Message);
            }

            return default;
        }

        public void SaveToPlayerPrefs(object saveData)
        {
            GPGSSaveLoadUtil.SaveToPlayerPrefs(saveData, saveKey);
        }

        public T LoadFromPlayerPrefs()
        {
            return (T)GPGSSaveLoadUtil.LoadFromPlayerPrefs(saveKey, typeof(T));
        }
    }
}