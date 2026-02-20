using UnityEngine;

public interface INavVisualizer
{
    void UpdateWithRectImmediate(RectTransform rect);
    void MatchRectScale(RectTransform rect);
    void ResetScale();
    RectTransform GetRect();
    Coroutine LerpWithRect(RectTransform rt);
}
