public static class NavState
{
    public static NavItem currentItem;
    public static bool inInputField;
    public static bool hoveringSelector;
    public static bool Scrolling;

    static NavItem hoveredItem;
    public static NavItem HoveredItem
    {
        get => hoveredItem;
        set
        {
            if (hoveredItem == value) return;
            lastHoveredItem = hoveredItem;
            hoveredItem = value;
        }
    }

    static NavItem lastHoveredItem;
    public static NavItem LastHoveredItem
    {
        get => lastHoveredItem;
        set { lastHoveredItem = value; }
    }

    public static void Init()
    {
        currentItem = null;
        inInputField = false;
        hoveredItem = null;
        lastHoveredItem = null;
        Scrolling = false;
    }
}
