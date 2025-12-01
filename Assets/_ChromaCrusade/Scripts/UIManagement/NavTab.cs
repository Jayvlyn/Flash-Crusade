using UnityEngine;

public class NavTab : NavItem
{
    [HideInInspector] public TabManager owner;
    [HideInInspector] public bool selected = false;

    public override void OnSelected()
    {
        if (selected) return;
        selected = true;

        base.OnSelected();
        owner?.SwitchToTab(this);
    }
}
