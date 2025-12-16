using UnityEngine;

public interface IVisualizer
{
    void HighlightCellImmediate(Vector2Int cell);
    void UpdateWithRectImmediate(RectTransform rect);
    void MatchRectScale(RectTransform rect);
    void ResetScale();
    void SetExpanded(bool expanded);
}
