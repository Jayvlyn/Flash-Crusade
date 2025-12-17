using UnityEngine;

public interface IPartDestroyer
{
    void DestroyPart(ShipPart part);
    void HandleUndoRoutine(bool wasPlaced,
        ShipPartData partData,
        Vector2Int partPosition,
        Vector2Int startCell,
        float rotation,
        bool xFlipped = false, bool yFlipped = false);
}
