using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SaveManager controller = (SaveManager)target;
        if (GUILayout.Button("Test Save and Load."))
        {
            controller.Test_SaveLoad_and_PrintToDebugLog();
        }
    }
}