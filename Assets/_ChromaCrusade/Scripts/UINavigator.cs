using UnityEngine;

public class UINavigator : Navigator
{
    [SerializeField] NavItem initialHoveredItem;

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

    protected override void Start()
    {
        base.Start();
    }

    public override void Init()
    {
        NavItem targetItem = null;
        if (lastHoveredItem != null) targetItem = lastHoveredItem;
        else if (initialHoveredItem != null) targetItem = initialHoveredItem;
        else targetItem = GetComponentInChildren<NavItem>();
        NavToItem(targetItem);

        visualizer.ResetRotation();
        visualizer.ResetScale();
    }

    public override void TriggerNav(Vector2 dir)
    {
        if (HoveredItem == null)
            return;

        NavItem next = null;

        if (dir.y > 0.5f) next = HoveredItem.navUp;
        else if (dir.y < -0.5f) next = HoveredItem.navDown;
        else if (dir.x < -0.5f) next = HoveredItem.navLeft;
        else if (dir.x > 0.5f) next = HoveredItem.navRight;

        if (next == null)
            return;

        NavToItem(next);
    }

    public void NavToItem(NavItem item)
    {
        if (item == null) return;
        HoveredItem = item;
        HoveredItem.OnHighlighted();
        visualizer.HighlightItem(HoveredItem);
    }
}
