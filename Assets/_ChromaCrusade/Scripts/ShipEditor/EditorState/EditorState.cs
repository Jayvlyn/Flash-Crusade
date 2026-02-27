using UnityEngine;

public static class EditorState
{
    public static NavMode navMode;
    public static ShipPart heldPart;
    public static bool midUndoDelete;
    public static bool midGrab;
    public static bool enteringGrid;
    public static bool creativeMode;

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
        navMode = NavMode.Item;
        heldPart = null;
        midUndoDelete = false;
        midGrab = false;
        enteringGrid = false;
        creativeMode = true; // false;
        currentGridCell = Vector2Int.zero;
    }
}
