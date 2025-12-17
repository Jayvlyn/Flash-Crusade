using UnityEngine;

public interface IPartPlacer
{
    void PlacePart(ShipPart part, Vector2Int cell);
    ShipPart GetHeldPart();
}
