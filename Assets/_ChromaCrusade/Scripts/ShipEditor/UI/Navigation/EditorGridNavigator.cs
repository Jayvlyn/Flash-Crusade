using UnityEngine;

public class EditorGridNavigator : Navigator, IGridNavigator
{
    protected EditorNavVisualizer editorVisualizer;

    [SerializeField] RectTransform centerGridCell;
    public IUINavigator uiNav;

    [SerializeField] Transform parent;

    void OnEnable()
    {
        EventBus.Subscribe<NewZoomLevelEvent>(OnNewZoomLevelEvent);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<NewZoomLevelEvent>(OnNewZoomLevelEvent);
    }

    public override void Init()
    {
        base.Init();

        if (editorVisualizer == null)
            editorVisualizer = visualizer as EditorNavVisualizer;

        editorVisualizer.centerGridCell = centerGridCell;

        visualizer.transform.SetParent(parent);
        visualizer.transform.localScale = Vector3.one;
        EditorState.enteringGrid = true; // when current cell is initialized the camera wont try to follow it with this
        NavToCell(EditorState.CurrentGridCell);

    }

    public void TriggerGridNav(Vector2 dir)
    {
        Vector2Int offset = new Vector2Int((int)dir.x, (int)dir.y);

        if (offset == Vector2Int.zero)
            return;

        Vector2Int newCell = EditorState.CurrentGridCell + offset;

        NavToCell(newCell);
    }

    void OnNewZoomLevelEvent(NewZoomLevelEvent e)
    {
        if (EditorState.navMode == NavMode.Grid) 
            editorVisualizer.HighlightCellImmediate(EditorState.CurrentGridCell);
    }

    public void NavToCell(Vector2Int cell)
    {
        NavState.currentItem = null;
        NavState.HoveredItem = null;
        EditorState.CurrentGridCell = cell;
        editorVisualizer.HighlightCell(EditorState.CurrentGridCell);
    }

    public void ResetGridPosition()
    {
        EditorState.CurrentGridCell = Vector2Int.zero;
    }

    public Vector2Int GetCurrentGridCell()
    {
        return EditorState.CurrentGridCell;
    }

    public void InitGridMode()
    {
        Init();
    }

    public void SwitchToItemMode()
    {
        if (EditorState.navMode == NavMode.Item) return;
        EventBus.Publish(new CancelCameraMovementEvent());
        EditorState.navMode = NavMode.Item;
        uiNav.Init();
    }
}
