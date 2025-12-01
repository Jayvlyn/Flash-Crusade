using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class NavVisualizer : MonoBehaviour
{
    public float transitionDuration = 0.12f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnHighlightNew(NavItem newItem)
    {
        Vector2 targetPos;
        Vector2 targetSize;
        GetWorldRectValues(newItem.rect, out targetPos, out targetSize);

        if (lerpRoutine != null)
            StopCoroutine(lerpRoutine);

        lerpRoutine = StartCoroutine(LerpTo(targetPos, targetSize, transitionDuration));
    }

    private void GetWorldRectValues(RectTransform target, out Vector2 pos, out Vector2 size)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        RectTransform parent = rect.parent as RectTransform;
        Vector3[] local = new Vector3[4];
        for (int i = 0; i < 4; i++)
            local[i] = parent.InverseTransformPoint(corners[i]);

        Vector2 bl = local[0];
        Vector2 tr = local[2];

        size = tr - bl;
        pos = bl + size * 0.5f;
    }

    private Coroutine lerpRoutine;
    private IEnumerator LerpTo(Vector2 targetPos, Vector2 targetSize, float duration)
    {
        Vector2 startPos = rect.anchoredPosition;
        Vector2 startSize = rect.sizeDelta;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float s = Mathf.SmoothStep(0, 1, t);

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, s);

            Vector2 newSize = Vector2.Lerp(startSize, targetSize, s);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);

            yield return null;
        }

        rect.anchoredPosition = targetPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
    }
}
