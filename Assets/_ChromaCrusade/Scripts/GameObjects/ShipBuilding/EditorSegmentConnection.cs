using UnityEngine;

public class EditorSegmentConnection
{
    private enum ConnectionState { Blocked, Disabled, Enabled }
    // Blocked = Red, Diabled = Yellow, Enabled = Green
    private ConnectionState connectionState = ConnectionState.Disabled;

    public void OnSelected()
    {
        if      (connectionState == ConnectionState.Disabled) connectionState = ConnectionState.Enabled;
        else if (connectionState == ConnectionState.Enabled)  connectionState = ConnectionState.Disabled;
    }
}
