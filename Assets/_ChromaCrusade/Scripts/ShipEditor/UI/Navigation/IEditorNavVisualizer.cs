using UnityEngine;

public interface IEditorNavVisualizer : INavVisualizer
{
    void HighlightCellImmediate(Vector2Int cell);
    void SetExpanded(bool expanded);
    void Flip(FlipAxis axis);
    void FlipImmediate(FlipAxis axis);
    void Rotate(float angle);
    void RotateImmediate(float angle);
}