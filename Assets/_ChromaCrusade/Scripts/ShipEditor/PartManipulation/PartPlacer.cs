using UnityEngine;

public class PartPlacer : MonoBehaviour, IPartPlacer
{
    [HideInInspector] public BuildArea buildArea;

    #region IPartPlacer

    public EditorShipPart GetHeldPart()
    {
        return EditorState.heldPart;
    }

    public void PlacePart(EditorShipPart part, Vector2Int cell)
    {
        buildArea.PlacePart(part, cell);
        part.OnPlaced(cell);
        EditorState.heldPart = null;
    }

    #endregion
}
