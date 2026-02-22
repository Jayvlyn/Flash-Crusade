using UnityEngine;

public class NavState
{
    public NavItem currentItem;
    public bool inInputField;

    NavItem hoveredItem;
    public NavItem HoveredItem
    {
        get => hoveredItem;
        set
        {
            if (hoveredItem == value) return;
            lastHoveredItem = hoveredItem;
            hoveredItem = value;
        }
    }

    NavItem lastHoveredItem;
    public NavItem LastHoveredItem
    {
        get => lastHoveredItem;
        set { lastHoveredItem = value; }
    }
}
