using UnityEngine;

public class EditorState
{
    public NavMode navMode = NavMode.Item;
    public ShipPart heldPart;
    public bool midUndoDelete;
    public bool midGrab;
    public bool inInputField;
    public Vector2Int currentGridCell;
    public NavItem currentItem;

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
