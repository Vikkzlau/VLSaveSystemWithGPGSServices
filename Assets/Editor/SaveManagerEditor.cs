using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveGame))]
public class SaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SaveGame controller = (SaveGame)target;
        GUILayout.Label("See Console for test result.");
        if (GUILayout.Button("Test Save and Load. (XmlSerializer/GPGS)"))
        {
            controller.Test_SaveLoad();
        }
        if (GUILayout.Button("Test Save and Load. (PlayerPrefs)"))
        {
            controller.Test_SaveLoad_PlayerPrefs();
        }
        if (GUILayout.Button("Test Xml ByteArray Conversion."))
        {
            controller.Test_XmlByteArrayConversion();
        }
    }
}