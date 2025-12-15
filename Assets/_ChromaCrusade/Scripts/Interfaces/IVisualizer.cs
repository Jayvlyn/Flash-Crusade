using UnityEngine;

public interface IVisualizer
{
    void UpdateWithRectImmediate(RectTransform rect);
    void MatchRectScale(RectTransform rect);
    void ResetScale();
}
