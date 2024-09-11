using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;

public class SaveManagerTest
{/*
    [UnityTest]
    public IEnumerator Test_SaveLoad()
    {
        GameObject gameObject = new();
        SaveManager saveManager = gameObject.AddComponent<SaveManager>();
        yield return null;
        SaveManager.SaveData saveData = new(1337);

        saveManager.Save(saveData, typeof(SaveManager.SaveData));
        SaveManager.SaveData loadedData = (SaveManager.SaveData)saveManager.Load(out _);
        
        Assert.AreEqual(saveData.HP, loadedData.HP);
    }

    [UnityTest]
    public IEnumerator Test_SaveLoad_PlayerPrefs()
    {
        GameObject gameObject = new();
        SaveManager saveManager = gameObject.AddComponent<SaveManager>();
        yield return null;
        SaveManager.SaveData saveData = new(1337);

        saveManager.SaveToPlayerPrefs(saveData);
        SaveManager.SaveData loadedData = (SaveManager.SaveData)saveManager.LoadFromPlayerPrefs();

        Assert.AreEqual(saveData.HP, loadedData.HP);
    }

    [Test]
    public void Test_XmlByteArrayConversion()
    {
        SaveManager.SaveData saveData = new(1337);

        byte[] savebyte = VLSaveSystemWithGPGSServices.GPGSSaveLoadUtil.ObjectToByteArray(saveData);
        SaveManager.SaveData returnedData = 
            (SaveManager.SaveData)VLSaveSystemWithGPGSServices.GPGSSaveLoadUtil.ByteArrayToObject(savebyte, typeof(SaveManager.SaveData));

        Assert.AreEqual(saveData.HP, returnedData.HP);
    }*/
}