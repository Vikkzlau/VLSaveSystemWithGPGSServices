using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;
using VLSaveSystemWithGPGSServices;

public class SaveManagerTest
{
    private const string savefileName = "/save1.dat";
    private const string saveKey = "save1";

    [UnityTest]
    public IEnumerator Test_SaveLoad()
    {
        GameObject gameObject = new();
        yield return null;
        SaveData saveData = new(1337);
        var saveManager = new SaveManagerUtil<SaveData>(savefileName, saveKey);

        saveManager.Save(saveData);
        SaveData loadedData = saveManager.Load(out _);
        
        Assert.AreEqual(saveData.HP, loadedData.HP);
    }

    [UnityTest]
    public IEnumerator Test_SaveLoad_PlayerPrefs()
    {
        GameObject gameObject = new();
        yield return null;
        SaveData saveData = new(1337);
        var saveManager = new SaveManagerUtil<SaveData>(savefileName, saveKey);

        saveManager.SaveToPlayerPrefs(saveData);
        SaveData loadedData = saveManager.LoadFromPlayerPrefs();

        Assert.AreEqual(saveData.HP, loadedData.HP);
    }

    [Test]
    public void Test_XmlByteArrayConversion()
    {
        SaveData saveData = new(1337);

        byte[] savebyte = VLSaveSystemWithGPGSServices.GPGSSaveLoadUtil.ObjectToByteArray(saveData);
        SaveData returnedData = 
            (SaveData)VLSaveSystemWithGPGSServices.GPGSSaveLoadUtil.ByteArrayToObject(savebyte, typeof(SaveData));

        Assert.AreEqual(saveData.HP, returnedData.HP);
    }
}