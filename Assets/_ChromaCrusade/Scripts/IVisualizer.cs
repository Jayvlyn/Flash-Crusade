using UnityEngine;

public interface IVisualizer
{
    void HighlightCellImmediate(Vector2Int cell);
    void UpdateWithRectImmediate(RectTransform rect);
    void MatchRectScale(RectTransform rect);
    void ResetScale();
    void SetExpanded(bool expanded);
    RectTransform GetRect();
    Coroutine LerpWithRect(RectTransform rt);

    void Flip(FlipAxis axis);
    void FlipImmediate(FlipAxis axis);
    void Rotate(float angle);
    void RotateImmediate(float angle);
}