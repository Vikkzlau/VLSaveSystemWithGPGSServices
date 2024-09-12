/*  VLSaveSystemWithGPGSServices (Save system for Unity projects, compatible with GPGS)
    Copyright (C) 2024  Vikkzlau

    VLSaveSystemWithGPGSServices is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    VLSaveSystemWithGPGSServices is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with VLSaveSystemWithGPGSServices.  If not, see <https://www.gnu.org/licenses/>.*/
using System;
using System.IO;
using UnityEngine;
//Install the Newtonsoft.Json in unity package manager. Install from git URL: "com.unity.nuget.newtonsoft-json"
using Newtonsoft.Json;
using UnityEditor;
using System.Xml.Serialization;
using JetBrains.Annotations;


#if UNITY_ANDROID
using GooglePlayGames;
#endif

namespace VLSaveSystemWithGPGSServices
{
    public enum SaveLoadMethod
    {
        NotLoaded,
        LoadedFromGPGS,
        LoadedFromXml,
        LoadedFromPlayerPrefs,
    }

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

        /// <summary>
        /// Saves object. Use LoadObject to load saved object.
        /// </summary>
        /// <param name="key">Saves are identified by the given key</param>
        public static void SaveObject(object saveData, string savePath, string key, Type castType)
        {
            FileStream file = File.Create(savePath);
            Save_Serialize(file, saveData, key, castType);
            file.Close();
        }

        public static object LoadObject(string savePath, string key, Type castType)
        {
            return LoadObject(savePath, key, castType, out _);
        }

        /// <summary>
        /// Use this to load object that is saved using SaveObject.
        /// </summary>
        /// <param name="key">Saves are identified by the given key</param>
        /// <param name="loadingMethod">return -1 = not loaded. 0 = loaded from GPGS. 1 = from Xml. 2 = from PlayerPrefs</param>
        /// <returns></returns>
        public static object LoadObject(string savePath, string key, Type castType, out SaveLoadMethod saveLoadMethod)
        {
            if (File.Exists(savePath))
            {
                FileStream file = File.Open(savePath, FileMode.Open);

                object obj = Load_Deserialize(file, key, castType, out saveLoadMethod);

                file.Close();

                if (obj != null)
                {
                    return obj;
                }
            }
            saveLoadMethod = SaveLoadMethod.NotLoaded;
            return null;
        }

        /// <summary>
        /// Serializes object then saves to GPGS or PlayerPrefs (if on WebGL).
        /// </summary>
        /// <param name="saveObj">objects can be passed here to be saved</param>
        /// <param name="key">Saves are identified by the given key</param>
        private static void Save_Serialize(FileStream serializationStream, object saveObj, string key, Type castType)
        {
            serializationStream.Serialize(saveObj, castType);
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.Log("WebGL platform. Saving to PlayerPrefs.");
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

        public static void SaveToPlayerPrefs(object saveObj, string key)
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
        /// <param name="loadingMethod">return -1 = not loaded. 0 = loaded from GPGS. 1 = from Xml. 2 = from PlayerPrefs</param>
        private static object Load_Deserialize(FileStream serializationStream, string key, Type castType, out SaveLoadMethod saveLoadMethod)
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
                        saveLoadMethod = SaveLoadMethod.LoadedFromGPGS;
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
                {
                    saveLoadMethod = SaveLoadMethod.LoadedFromXml;
                    return obj;
                }
            }

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                obj = LoadFromPlayerPrefs(key, castType);
                if (obj != null)
                {
                    saveLoadMethod = SaveLoadMethod.LoadedFromPlayerPrefs;
                    return obj;
                }
            }
            saveLoadMethod = SaveLoadMethod.NotLoaded;
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

        public static object LoadFromPlayerPrefs(string key, Type castType)
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

        //return -1 = not loaded. 0 = loaded from GPGS. 1 = from Xml. 2 = from PlayerPrefs
        public static string GetLoadingMethod(SaveLoadMethod saveLoadMethod)
        {
            string lm = (int)saveLoadMethod switch
            {
                -1 => "Not Loaded",
                0 => "Loaded from GPGS",
                1 => "Loaded from Xml",
                2 => "Loaded from PlayerPrefs",
                _ => "Invalid"
            };
            return lm;
        }
    }
}
