using UnityEngine;

public interface IInventoryManager
{
    bool TryTakePart(ShipPartData data, out ShipPart part);
    void AddPart(ShipPartData data);
    void SetPartToDefaultStart(ShipPart part);
}
