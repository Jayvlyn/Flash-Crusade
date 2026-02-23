using UnityEngine;

public class UINavigator : Navigator, IUINavigator
{
    [SerializeField] Transform visualizerParent;
    [SerializeField] NavItem initialHoveredItem;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        visualizer.transform.SetParent(visualizerParent);
        visualizer.transform.localScale = Vector3.one;
        NavItem targetItem = null;
        if (NavState.LastHoveredItem != null) targetItem = NavState.LastHoveredItem;
        else if (initialHoveredItem != null) targetItem = initialHoveredItem;
        else targetItem = GetComponentInChildren<NavItem>();
        NavToItem(targetItem);

        visualizer.ResetScale();
    }

    public void NavToItem(NavItem item)
    {
        if (item == null) return;
        NavState.HoveredItem = item;
        NavState.HoveredItem.OnHighlighted();
        visualizer.HighlightItem(NavState.HoveredItem);
    }

    public virtual void SwitchOff() {}

    public void TriggerItemNav(Vector2 dir)
    {
        if (NavState.HoveredItem == null)
            return;

        NavItem next = null;

        if (dir.y > 0.5f) next = NavState.HoveredItem.navUp;
        else if (dir.y < -0.5f) next = NavState.HoveredItem.navDown;
        else if (dir.x < -0.5f) next = NavState.HoveredItem.navLeft;
        else if (dir.x > 0.5f) next = NavState.HoveredItem.navRight;

        if (next == null)
            return;

        NavToItem(next);
    }
}
