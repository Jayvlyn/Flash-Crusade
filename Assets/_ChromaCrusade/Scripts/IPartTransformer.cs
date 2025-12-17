using UnityEngine;

public interface IPartTransformer
{
    void RotatePart(float angle);
    void FlipPart(FlipAxis axis);

    void RestorePartTransformations(float rotation, bool xFlipped = false, bool yFlipped = false);
}
