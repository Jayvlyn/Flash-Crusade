using System.Collections;
using UnityEngine;

public interface IPartGrabber
{
    EditorShipPart GrabFromGrid(Vector2Int cell);

    void GrabImmediate(EditorShipPart part, bool fromInv);

    void GrabFrameLate(EditorShipPart part, bool fromInv);

    void GrabWithLerp(EditorShipPart part, bool fromInv);
}
