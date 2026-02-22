using UnityEngine;

public class EditorState
{
    public EditorState()
    {
        navState = new NavState();
    }

    public NavState navState;
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

    public NavItem currentItem
    {
        get { return navState.currentItem; }
        set { navState.currentItem = value; }
    }

    public NavItem HoveredItem
    {
        get { return navState.HoveredItem; }
        set { navState.HoveredItem = value; }
    }

    public NavItem LastHoveredItem
    {
        get { return navState.LastHoveredItem; }
        set { navState.LastHoveredItem = value; }
    }

    public bool inInputField
    {
        get { return navState.inInputField; }
        set { navState.inInputField = value; }
    }
}
