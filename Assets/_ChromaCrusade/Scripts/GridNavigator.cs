using UnityEngine;

public class GridNavigator : Navigator
{
    [SerializeField] RectTransform centerGridCell;

    private Vector2Int currentGridCell;
    public Vector2Int CurrentGridCell { 
        get { return currentGridCell; } 
        set { currentGridCell = value; }
    }

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
        NavToCell(currentGridCell);
    }

    public override void TriggerNav(Vector2 dir)
    {
        Vector2Int offset = new Vector2Int((int)dir.x, (int)dir.y);

        if (offset == Vector2Int.zero)
            return;

        Vector2Int newCell = CurrentGridCell + offset;

        NavToCell(newCell);
    }

    void OnNewZoomLevelEvent(NewZoomLevelEvent e)
    {
        visualizer.HighlightCellImmediate(currentGridCell);
    }

    public void NavToCell(Vector2Int cell)
    {
        currentGridCell = cell;
        visualizer.HighlightCell(currentGridCell);
    }

    public void ResetGridPosition()
    {
        currentGridCell = Vector2Int.zero;
    }

}
