using UnityEngine;

public interface INavVisualizer
{
    void UpdateWithRectImmediate(RectTransform rect);
    void MatchRectScale(RectTransform rect);
    void ResetScale();
    RectTransform GetRect();
    Coroutine LerpWithRect(RectTransform rt);
    void HighlightItem(NavItem newItem);
    void HighlightItemImmediate(NavItem newItem);
    void HighlightItemLerp(NavItem newItem);
}
