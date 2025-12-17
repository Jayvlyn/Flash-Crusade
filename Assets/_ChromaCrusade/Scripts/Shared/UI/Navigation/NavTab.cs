using UnityEngine;

public class NavTab : NavItem
{
    [HideInInspector] public TabManager owner;
    [HideInInspector] public bool selected = false;

    [HideInInspector] public Vector2 anchorMinInactive;
    [HideInInspector] public Vector2 anchorMaxInactive;
    [HideInInspector] public Vector2 anchorMinActive;
    [HideInInspector] public Vector2 anchorMaxActive;

    public override void OnSelected()
    {
        if (selected) return;
        selected = true;

        base.OnSelected();
        owner?.SwitchToTab(this);
    }
}
