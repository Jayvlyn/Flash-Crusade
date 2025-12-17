using UnityEngine;

public interface IInventoryManager
{
    bool TryTakePart(ShipPartData data, out EditorShipPart part);
    void AddPart(ShipPartData data);
    void SetPartToDefaultStart(EditorShipPart part);
}
