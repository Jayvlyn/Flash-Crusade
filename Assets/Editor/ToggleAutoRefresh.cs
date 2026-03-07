using UnityEditor;
using UnityEngine;

public static class ToggleAutoRefresh
{
    const string Key = "kAutoRefreshMode";

    [MenuItem("Tools/Toggle Auto Refresh %#r")]
    static void Toggle()
    {
        int mode = EditorPrefs.GetInt(Key, 1);

        if (mode == 0)
        {
            EditorPrefs.SetInt(Key, 1);   // enable
            Debug.Log("Auto Refresh Enabled");
        }
        else
        {
            EditorPrefs.SetInt(Key, 0);   // disable
            Debug.Log("Auto Refresh Disabled");
        }

        AssetDatabase.Refresh();
    }
}