using System;
using System.IO;
using UnityEngine;
//Install the Newtonsoft.Json in unity package manager. Install from git URL: "com.unity.nuget.newtonsoft-json"
using Newtonsoft.Json;
using UnityEditor;
using System.Xml.Serialization;

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

        public static void SaveObject(object saveData, string savePath, string key, Type castType)
        {
            FileStream file = File.Create(savePath);
            Save_Serialize(file, saveData, key, castType);
            file.Close();
        }

        public static object LoadObject(string savePath, string key, Type castType)
        {
            if (File.Exists(savePath))
            {
                FileStream file = File.Open(savePath, FileMode.Open);

                object obj = Load_Deserialize(file, key, castType);

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
        private static void Save_Serialize(FileStream serializationStream, object saveObj, string key, Type castType)
        {
            serializationStream.Serialize(saveObj, castType);
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
            
            byte[] byteArray = ObjectToByteArray(saveObj);
            GPGSManager.Instance.SaveGame(key, byteArray);
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
        private static object Load_Deserialize(FileStream serializationStream, string key, Type castType)
        {
#if UNITY_ANDROID
            try
            {
                if (Application.platform == RuntimePlatform.Android && PlayGamesPlatform.Instance.IsAuthenticated())
                {
                    object tempObj;
                    TimeSpan loadedPlayTime = TimeSpan.Zero;
                    tempObj = LoadFromGPGS(key, out loadedPlayTime, castType);
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
                obj = serializationStream.Deserialize(castType);
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
        private static object LoadFromGPGS(string key, out TimeSpan loadedPlayTime, Type castType)
        {
            byte[] loadedGameData = GPGSManager.Instance.LoadGame(key, out loadedPlayTime);
            object obj = ByteArrayToObject(loadedGameData, castType );
            return obj;
        }
#endif

        private static object LoadFromPlayerPrefs(string key, Type castType)
        {
            string byteString = PlayerPrefs.GetString(key);
            //Debug.Log("[LoadFromPlayerPrefs()] str:" + byteString);
            object obj1 = JsonConvert.DeserializeObject(byteString, castType);

            return obj1;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            var serializer = new XmlSerializer(obj.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object ByteArrayToObject(byte[] arrBytes, Type targetType)
        {
            var serializer = new XmlSerializer(targetType);
            using (var memStream = new MemoryStream(arrBytes))
            {
                return serializer.Deserialize(memStream);
            }
        }

        // Serialize the object to an existing FileStream
        public static void Serialize(this FileStream fileStream, object saveObj, Type targetType)
        {
            var serializer = new XmlSerializer(targetType);
            serializer.Serialize(fileStream, saveObj);
        }

        // Deserialize an object from an existing FileStream
        public static object Deserialize(this FileStream fileStream, Type targetType)
        {
            var serializer = new XmlSerializer(targetType);
            return serializer.Deserialize(fileStream);
        }
    }
}
