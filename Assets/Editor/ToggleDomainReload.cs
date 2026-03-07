using UnityEditor;
using UnityEngine;

public static class ToggleDomainReload
{
    [MenuItem("Tools/Toggle Domain Reload %#d")] // Ctrl+Shift+D
    static void Toggle()
    {
        if (!EditorSettings.enterPlayModeOptionsEnabled)
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            Debug.Log("Domain Reload Disabled");
        }
        else if (EditorSettings.enterPlayModeOptions == EnterPlayModeOptions.DisableDomainReload)
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
            Debug.Log("Domain Reload Enabled");
        }
        else
        {
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
            Debug.Log("Domain Reload Enabled");
        }
    }
}