using UnityEngine;
using VLSaveSystemWithGPGSServices;

public class SaveGame : MonoBehaviour
{
    public SaveManagerUtil<SaveData> saveManager;
    // e.g. "/save1.dat"
    private const string savefileName = "/save1.dat";
    // e.g. "save1"
    private const string saveKey = "save1";

    private void Start()
    {
        saveManager = new SaveManagerUtil<SaveData>(savefileName, saveKey);
    }

    // Methods below are for testing purposes only.
    // These are simpler than using unit testing framework, buttons to call these can be found on Inspector of gameobject that has SaveManager.
    // Similar tests are also done using Unity Tests in SaveManagerTest.cs.
    public void Test_SaveLoad()
    {
        SaveData saveData = new(1337);
        saveManager.Save(saveData);
        SaveData loadedData = saveManager.Load(out SaveLoadMethod saveLoadMethod);
        if (saveLoadMethod == SaveLoadMethod.LoadedFromXml)
            Debug.Log("LOADED. HP = " + loadedData.HP + " | Loaded from Xml. Path: " + saveManager.savePath);
        else
            Debug.Log("LOADED. HP = " + loadedData.HP + " | " + GPGSSaveLoadUtil.GetLoadingMethod(saveLoadMethod));
    }

    public void Test_SaveLoad_PlayerPrefs()
    {
        SaveData saveData = new(1337);
        saveManager.SaveToPlayerPrefs(saveData);
        SaveData loadedData = saveManager.LoadFromPlayerPrefs();
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
