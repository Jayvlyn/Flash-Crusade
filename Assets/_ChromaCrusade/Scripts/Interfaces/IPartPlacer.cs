using UnityEngine;

public interface IPartPlacer
{
    void PlacePart(EditorShipPart part, Vector2Int cell);
}
