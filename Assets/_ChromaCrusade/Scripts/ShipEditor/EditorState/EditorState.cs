using UnityEngine;

public class EditorState
{
    public NavMode navMode = NavMode.Item;
    public ShipPart heldPart;
    public bool midUndoDelete;
    public bool midGrab;
    public bool inInputField;
    public NavItem currentItem;

    private Vector2Int currentGridCell;
    public Vector2Int CurrentGridCell
    {
        get { return currentGridCell; }
        set 
        {
            currentGridCell = value;
            EventBus.Publish(new NewGridCellEvent { cell = currentGridCell });
        }
    }


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
}
