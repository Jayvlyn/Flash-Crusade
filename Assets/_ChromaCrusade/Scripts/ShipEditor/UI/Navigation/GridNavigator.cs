using UnityEngine;

public class GridNavigator : Navigator, IGridNavigator
{
    [SerializeField] RectTransform centerGridCell;
    public IUINavigator uiNav;

    void OnEnable()
    {
        EventBus.Subscribe<NewZoomLevelEvent>(OnNewZoomLevelEvent);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<NewZoomLevelEvent>(OnNewZoomLevelEvent);
    }

    protected override void Start()
    {
        base.Start();
        visualizer.centerGridCell = centerGridCell;
    }

    public override void Init()
    {
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
        if(EditorState.navMode == NavMode.Grid) 
            visualizer.HighlightCellImmediate(EditorState.CurrentGridCell);
    }

    public void NavToCell(Vector2Int cell)
    {
        EditorState.currentItem = null;
        EditorState.HoveredItem = null;
        EditorState.CurrentGridCell = cell;
        visualizer.HighlightCell(EditorState.CurrentGridCell);
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
        EditorState.navMode = NavMode.Item;
        uiNav.InitItemMode();
    }
}
