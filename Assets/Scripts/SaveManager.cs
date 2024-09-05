using System;
using UnityEngine;
using VLSaveSystemWithGPGSServices;

public class SaveManager : MonoBehaviour
{
    // Example class to be used as save object. Must be marked as Serializable.
    [Serializable]
    public class SaveData
    {
        public int HP = 0;

        public SaveData()
        {
            HP = 0;
        }
        public SaveData(int hp)
        {
            HP = hp;
        }
    }

    private const string savefileName = "/save.dat";
    private string savePath;
    private const string saveKey = "dat";

    void Start()
    {
        savePath = Application.persistentDataPath + savefileName;
        GPGSSaveLoadUtil.InitializeAssets();
    }

    public void Save(object saveData, Type type) 
    {
        GPGSSaveLoadUtil.SaveObject(saveData, savePath, saveKey, type);
    }

    public object Load(out SaveLoadMethod saveLoadMethod)
    {
        object loadedObj = GPGSSaveLoadUtil.LoadObject(savePath, saveKey, typeof(SaveData), out saveLoadMethod);
        SaveData loadData = null;

        // using try catch here, since GPGS loading may have problems.
        try
        {
            loadData = (SaveData)loadedObj;
        }
        catch (Exception e)
        {
            Debug.Log("Object type cast failed during loading. " + e.Message, this);
        }

        if (loadData != null)
        {
            // Data loaded successfully.
            return loadData;
        }
        return null;
    }

    public void SaveToPlayerPrefs(object saveData)
    {
        GPGSSaveLoadUtil.SaveToPlayerPrefs(saveData, saveKey);
    }

    public object LoadFromPlayerPrefs()
    {
        return GPGSSaveLoadUtil.LoadFromPlayerPrefs(saveKey, typeof(SaveData));
    }
        
    // Methods below are for testing purposes only.
    // These are simpler than using unit testing framework, buttons to call these can be found on Inspector of gameobject that has SaveManager.
    // Similar tests are also done using Unity Tests in SaveManagerTest.cs.
    public void Test_SaveLoad()
    {
        SaveData saveData = new(1337);
        Save(saveData, typeof(SaveData));
        SaveData loadedData = (SaveData)Load(out SaveLoadMethod saveLoadMethod);
        if (saveLoadMethod == SaveLoadMethod.LoadedFromXml)
            Debug.Log("LOADED. HP = " + loadedData.HP + " | Loaded from Xml. Path: " + savePath);
        else
            Debug.Log("LOADED. HP = " + loadedData.HP + " | " + GPGSSaveLoadUtil.GetLoadingMethod(saveLoadMethod));
    }

    public void Test_SaveLoad_PlayerPrefs()
    {
        SaveData saveData = new(1337);
        SaveToPlayerPrefs(saveData);
        SaveData loadedData = (SaveData)LoadFromPlayerPrefs();
        Debug.Log("LOADED. HP = " + loadedData.HP + " | Loaded from PlayerPrefs.");
    }

    public void Test_XmlByteArrayConversion()
    {
        SaveData saveData = new(1337);
        byte[] savebyte = GPGSSaveLoadUtil.ObjectToByteArray(saveData);
        SaveData returnedData = (SaveData)GPGSSaveLoadUtil.ByteArrayToObject(savebyte, typeof(SaveData));
        Debug.Log("LOADED: " + returnedData.HP);
    }
}