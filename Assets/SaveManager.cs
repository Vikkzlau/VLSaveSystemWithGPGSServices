using System;
using UnityEngine;
using VLSaveSystemWithGPGSServices;

public class SaveManager : MonoBehaviour
{
    private const string savefileName = "/save.dat";
    private string savePath;
    private const string saveKey = "dat";

    void Start()
    {
        savePath = Application.persistentDataPath + savefileName;
        GPGSSaveLoadUtil.InitializeAssets();
    }

    public void Save() 
    {
        SaveData saveData = new (1337);
        GPGSSaveLoadUtil.SaveObject(saveData, savePath, saveKey, typeof(SaveData));
    }

    public void Load()
    {
        object loadedObj = GPGSSaveLoadUtil.LoadObject(savePath, saveKey, typeof(SaveData));
        SaveData loadData = null;
        bool succeeded = false;

        try
        {
            loadData = (SaveData)loadedObj;
            succeeded = true;
        }
        catch
        {   Debug.Log("obj type cast failed", this); }

        if (succeeded)
        {
            Debug.Log("Loaded. HP = " + loadData.HP);

            // Data loaded successfully. Insert code here to pass the data (such as HP) somewhere for use.
        }
    }

    public void Test_SaveLoad_and_PrintToDebugLog()
    {
        Save();
        Load();
    }

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
}
