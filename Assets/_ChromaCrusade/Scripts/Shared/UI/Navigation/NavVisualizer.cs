using System.Collections;
using UnityEngine;

public class NavVisualizer : MonoBehaviour, INavVisualizer
{
    [Header("Settings")]
    public float transitionDuration = 0.12f;

    public bool IsLerping => lerpRoutine != null;
    protected Coroutine lerpRoutine;

    protected RectTransform rect;
    public RectTransform GetRect()
    {
        return rect;
    }

    public Coroutine LerpWithRect(RectTransform rt)
    {
        CancelLerp();
        return lerpRoutine = StartCoroutine(LerpRect(rt));
    }

    public void MatchRectScale(RectTransform rect)
    {
        var scale = this.rect.localScale;

        if (rect.localScale.x < 0)
            scale.x = -scale.x;

        if (rect.localScale.y < 0)
            scale.y = -scale.y;

        this.rect.localScale = scale;
    }

    public void ResetScale()
    {
        var scale = rect.localScale;
        scale.x = Mathf.Abs(scale.x);
        scale.y = Mathf.Abs(scale.y);
        rect.localScale = scale;
    }


    public void UpdateWithRectImmediate(RectTransform rect)
    {
        if (rect == null) return;

        GetWorldRectValues(rect, out Vector2 targetPos, out Vector2 targetSize);

        this.rect.anchoredPosition = targetPos;
        this.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        this.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
        this.rect.localEulerAngles = rect.localEulerAngles;
    }

    protected void GetWorldRectValues(RectTransform target, out Vector2 pos, out Vector2 size)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        RectTransform parent = rect.parent as RectTransform;
        Vector3[] local = new Vector3[4];
        for (int i = 0; i < 4; i++)
            local[i] = parent.InverseTransformPoint(corners[i]);

        float minX = Mathf.Min(local[0].x, Mathf.Min(local[1].x, Mathf.Min(local[2].x, local[3].x)));
        float maxX = Mathf.Max(local[0].x, Mathf.Max(local[1].x, Mathf.Max(local[2].x, local[3].x)));

        float minY = Mathf.Min(local[0].y, Mathf.Min(local[1].y, Mathf.Min(local[2].y, local[3].y)));
        float maxY = Mathf.Max(local[0].y, Mathf.Max(local[1].y, Mathf.Max(local[2].y, local[3].y)));

        size = new Vector2(maxX - minX, maxY - minY);
        pos = new Vector2(minX + size.x * 0.5f, minY + size.y * 0.5f);
    }

    public void CancelLerp()
    {
        if (IsLerping)
        {
            StopCoroutine(lerpRoutine);
            lerpRoutine = null;
        }
    }


    protected IEnumerator LerpRect(RectTransform rt)
    {
        if (rt == null) yield break;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 startSize = rect.sizeDelta;
        Vector2 startRot = rect.localEulerAngles;

        GetWorldRectValues(rt, out Vector2 targetPos, out Vector2 targetSize);

        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.Clamp01(t / transitionDuration);
            s = Mathf.SmoothStep(0, 1, s);

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, s);

            Vector2 size = Vector2.Lerp(startSize, targetSize, s);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            rect.rotation = Quaternion.Lerp(
                Quaternion.Euler(startRot),
                Quaternion.Euler(rt.localEulerAngles),
                s
            );

            yield return null;
        }

        rect.anchoredPosition = targetPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
        rect.rotation = Quaternion.Euler(rt.localEulerAngles);

        lerpRoutine = null;
    }
}
