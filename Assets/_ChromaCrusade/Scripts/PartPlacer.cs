using UnityEngine;

public class PartPlacer : MonoBehaviour, IPartPlacer
{
    [HideInInspector] public BuildArea buildArea;
    public EditorState EditorState { get; set; }

    #region IPartPlacer

    public ShipPart GetHeldPart()
    {
        return EditorState.heldPart;
    }

    public void PlacePart(ShipPart part, Vector2Int cell)
    {
        buildArea.PlacePart(part, cell);
        part.OnPlaced(cell, buildArea);
        EditorState.heldPart = null;
    }

    #endregion
}
