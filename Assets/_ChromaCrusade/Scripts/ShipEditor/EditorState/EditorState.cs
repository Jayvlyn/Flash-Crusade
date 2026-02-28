using UnityEngine;

public static class EditorState
{
    public static EditorContext context;
    public static NavMode navMode;
    public static ShipPart heldPart;
    public static bool midUndoDelete;
    public static bool midGrab;
    public static bool enteringGrid;
    public static bool Scrolling;

    private static Vector2Int currentGridCell;
    public static Vector2Int CurrentGridCell
    {
        get { return currentGridCell; }
        set 
        {
            currentGridCell = value;
            if(!enteringGrid) EventBus.Publish(new NewGridCellEvent { cell = currentGridCell });
            enteringGrid = false;
        }
    }

    public static void Init()
    {
        context = EditorContext.Creative;
        navMode = NavMode.Item;
        heldPart = null;
        midUndoDelete = false;
        midGrab = false;
        enteringGrid = false;
        Scrolling = false;
        currentGridCell = Vector2Int.zero;
    }
}

public enum EditorContext
{
    StartGame,
    MidGame,
    Creative
}
