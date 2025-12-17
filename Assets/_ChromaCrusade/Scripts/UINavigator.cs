using UnityEngine;

public class UINavigator : Navigator, IUINavigator
{
    [SerializeField] NavItem initialHoveredItem;
    public IGridNavigator gridNav;

    protected override void Start()
    {
        base.Start();
    }

    public override void Init()
    {
        NavItem targetItem = null;
        if (EditorState.LastHoveredItem != null) targetItem = EditorState.LastHoveredItem;
        else if (initialHoveredItem != null) targetItem = initialHoveredItem;
        else targetItem = GetComponentInChildren<NavItem>();
        NavToItem(targetItem);

        visualizer.ResetRotation();
        visualizer.ResetScale();
    }

    public void NavToItem(NavItem item)
    {
        if (item == null) return;
        EditorState.HoveredItem = item;
        EditorState.HoveredItem.OnHighlighted();
        visualizer.HighlightItem(EditorState.HoveredItem);
    }

    public void InitItemMode()
    {
        Init();
    }

    public void TriggerItemNav(Vector2 dir)
    {
        if (EditorState.HoveredItem == null)
            return;

        NavItem next = null;

        if (dir.y > 0.5f) next = EditorState.HoveredItem.navUp;
        else if (dir.y < -0.5f) next = EditorState.HoveredItem.navDown;
        else if (dir.x < -0.5f) next = EditorState.HoveredItem.navLeft;
        else if (dir.x > 0.5f) next = EditorState.HoveredItem.navRight;

        if (next == null)
            return;

        NavToItem(next);
    }

    public void SwitchToGridMode()
    {
        if (EditorState.navMode == NavMode.Grid) return;
        EditorState.navMode = NavMode.Grid;
        gridNav.InitGridMode();
    }
}
