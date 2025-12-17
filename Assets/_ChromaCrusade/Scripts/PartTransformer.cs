using UnityEngine;

public class PartTransformer : MonoBehaviour, IPartTransformer
{
    public EditorState EditorState { get; set; }

    public IVisualizer visualizer;

    public void FlipPart(FlipAxis axis)
    {
        if (EditorState.heldPart.Rotation == 90 || EditorState.heldPart.Rotation == 270)
        {
            if (axis == FlipAxis.Horizontal) axis = FlipAxis.Vertical;
            else axis = FlipAxis.Horizontal;
        }
        EditorState.heldPart.Flip(axis);
        visualizer.Flip(axis);
    }

    public void RestorePartTransformations(float rotation, bool xFlipped = false, bool yFlipped = false)
    {
        if (xFlipped) FlipPartImmediate(FlipAxis.Horizontal);
        if (yFlipped) FlipPartImmediate(FlipAxis.Vertical);
        if (rotation != 0) RotatePartImmediate(rotation);
    }

    public void RotatePart(float angle)
    {
        EditorState.heldPart.Rotate(angle);
        visualizer.Rotate(angle);
    }

    void FlipPartImmediate(FlipAxis axis)
    {
        if (EditorState.heldPart.Rotation == 90 || EditorState.heldPart.Rotation == 270)
        {
            if (axis == FlipAxis.Horizontal) axis = FlipAxis.Vertical;
            else axis = FlipAxis.Horizontal;
        }
        EditorState.heldPart.Flip(axis);
        visualizer.FlipImmediate(axis);
    }

    void RotatePartImmediate(float angle)
    {
        EditorState.heldPart.Rotate(angle);
        visualizer.RotateImmediate(angle);
    }
}
