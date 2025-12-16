using UnityEngine;

public interface IVisualizer
{
    void HighlightCellImmediate(Vector2Int cell, bool expanded = false);
    void UpdateWithRectImmediate(RectTransform rect);
    void MatchRectScale(RectTransform rect);
    void ResetScale();
}
