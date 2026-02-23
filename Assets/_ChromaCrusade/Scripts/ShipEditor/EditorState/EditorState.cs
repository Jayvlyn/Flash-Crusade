using UnityEngine;

public class EditorState
{
    public NavMode navMode = NavMode.Item;
    public ShipPart heldPart;
    public bool midUndoDelete;
    public bool midGrab;
    public bool enteringGrid = false;

    private Vector2Int currentGridCell;
    public Vector2Int CurrentGridCell
    {
        get { return currentGridCell; }
        set 
        {
            currentGridCell = value;
            if(!enteringGrid) EventBus.Publish(new NewGridCellEvent { cell = currentGridCell });
            enteringGrid = false;
        }
    }
}
