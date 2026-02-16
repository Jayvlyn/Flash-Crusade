using UnityEngine;

public class EditorNavTab : NavTab
{
    public override void OnSelected()
    {
        if (InventoryManager.Scrolling) return;
        base.OnSelected();
    }
}
