using UnityEngine;

public class EditorNavTab : NavTab
{
    public override void OnSelected()
    {
        if (NavState.Scrolling) return;
        if (EditorState.VisualizerLerping && NavState.hoveringSelector) return;
        base.OnSelected();
    }
}
