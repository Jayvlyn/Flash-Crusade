using System.Collections;
using UnityEngine;

public interface IPartGrabber
{
    ShipPart GrabFromGrid(Vector2Int cell);

    void GrabImmediate(ShipPart part, bool fromInv);

    void GrabFrameLate(ShipPart part, bool fromInv);

    void GrabWithLerp(ShipPart part, bool fromInv);
}
