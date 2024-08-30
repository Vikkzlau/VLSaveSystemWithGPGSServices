using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
//Install the Newtonsoft.Json in unity package manager. Install from git URL: "com.unity.nuget.newtonsoft-json"
using Newtonsoft.Json;
using UnityEditor;

#if UNITY_ANDROID
using GooglePlayGames;
#endif

namespace VLSaveSystemWithGPGSServices
{
    public static class GPGSSaveLoadUtil
    {
        public static void InitializeAssets()
        {
            if (GPGSManager.Instance == null)
            {
                GPGSManager asset = ScriptableObject.CreateInstance<GPGSManager>();
                AssetDatabase.CreateAsset(asset, "Assets/GPGSManager.asset");
            }
            if (PlayTimeManager.Instance == null)
            {
                PlayTimeManager asset = ScriptableObject.CreateInstance<PlayTimeManager>();
                AssetDatabase.CreateAsset(asset, "Assets/PlayTimeManager.asset");
            }
        }

        public static void SaveObject(object saveData, string savePath, string key)
        {
            BinaryFormatter formatter = new();
            FileStream file = File.Create(savePath);
            formatter.Save_Serialize(file, saveData, key);
            file.Close();
        }

        public static object LoadObject(string savePath, string key, Type castType)
        {
            if (File.Exists(savePath))
            {
                BinaryFormatter formatter = new();
                FileStream file = File.Open(savePath, FileMode.Open);

                object obj = formatter.Load_Deserialize(file, key, castType);

                file.Close();

                if (obj != null)
                {
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Serializes object then saves to GPGS or PlayerPrefs (if on WebGL).
        /// </summary>
        /// <param name="b"></param>
        /// <param name="serializationStream"></param>
        /// <param name="saveObj">objects can be passed here to be saved</param>
        /// <param name="key">Saves are identified by the given key</param>
        private static void Save_Serialize(this BinaryFormatter b, Stream serializationStream, object saveObj, string key)
        {
            b.Serialize(serializationStream, saveObj);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.Log("WebGL platform.");
                SaveToPlayerPrefs(saveObj, key);
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                SaveToGPGS(saveObj, key);
            }
        }

        private static void SaveToGPGS(object saveObj, string key)
        {
#if UNITY_ANDROID
            if (!PlayGamesPlatform.Instance.IsAuthenticated())
                return;
            using (MemoryStream memoryStream = new())
            {
                BinaryFormatter binaryFormatter = new();
                binaryFormatter.Serialize(memoryStream, saveObj);
                byte[] byteArray = memoryStream.ToArray();
                GPGSManager.Instance.SaveGame(key, byteArray);
            }
#endif
        }

        private static void SaveToPlayerPrefs(object saveObj, string key)
        {
            String str = JsonConvert.SerializeObject(saveObj);
            //Debug.Log("[SaveToPlayerPrefs()] str:" + str);

            PlayerPrefs.SetString(key, str);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads object from GPGS (if Android) or PlayerPrefs (if platform is WebGL) then deserializes object.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="serializationStream"></param>
        /// <param name="key">Saves are identified by the given key</param>
        private static object Load_Deserialize(this BinaryFormatter b, Stream serializationStream, string key, Type castType)
        {
#if UNITY_ANDROID
            try
            {
                if (Application.platform == RuntimePlatform.Android && PlayGamesPlatform.Instance.IsAuthenticated())
                {
                    object tempObj;
                    TimeSpan loadedPlayTime = TimeSpan.Zero;
                    tempObj = LoadFromGPGS(key, out loadedPlayTime);
                    if (tempObj != null && PlayTimeManager.Instance.GetLoadedTimeSpan() < loadedPlayTime)
                    {
                        return tempObj;
                    }
                }
            }
            catch { }
#endif

            object obj;
            if (Application.platform != RuntimePlatform.WebGLPlayer && serializationStream.Length > 0)
            {
                obj = b.Deserialize(serializationStream);
                //Debug.Log(key + " deserialized from file");
                if (obj != null)
                    return obj;
            }

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                obj = LoadFromPlayerPrefs(key, castType);
                if (obj != null)
                    return obj;
            }

            return null;
        }

#if UNITY_ANDROID
        private static object LoadFromGPGS(string key, out TimeSpan loadedPlayTime)
        {

            byte[] loadedGameData = GPGSManager.Instance.LoadGame(key, out loadedPlayTime);
            using (MemoryStream memoryStream = new(loadedGameData))
            {
                BinaryFormatter binaryFormatter = new();
                object obj = binaryFormatter.Deserialize(memoryStream);
                return obj;
            }
        }
#endif

        private static object LoadFromPlayerPrefs(string key, Type castType)
        {
            string byteString = PlayerPrefs.GetString(key);
            //Debug.Log("[LoadFromPlayerPrefs()] str:" + byteString);
            object obj1 = JsonConvert.DeserializeObject(byteString, castType);

            return obj1;
        }
    }
}
