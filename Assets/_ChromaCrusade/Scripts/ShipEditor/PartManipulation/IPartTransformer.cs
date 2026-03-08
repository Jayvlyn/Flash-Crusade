using UnityEngine;

public interface IPartTransformer
{
    void RotatePart(float angle);
    void FlipPart(FlipAxis axis);
    void RestoreHeldPartTransformations(float rotation, bool xFlipped = false, bool yFlipped = false);
    void RestorePartTransformations(EditorShipPart part, float rotation, bool xFlipped = false, bool yFlipped = false);
}
