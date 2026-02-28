using UnityEngine;

public class EditorNavTab : NavTab
{
    public override void OnSelected()
    {
        if (EditorState.Scrolling) return;
        base.OnSelected();
    }
}
