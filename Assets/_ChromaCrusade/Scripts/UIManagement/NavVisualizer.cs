using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class NavVisualizer : MonoBehaviour
{
    [Header("Settings")] 
    float transitionDuration = 0.12f;

    [HideInInspector] public RectTransform centerGridCell;
    [HideInInspector] public RectTransform rect;
    private NavItem currentItem;
    private Coroutine lerpRoutine;
    private Coroutine rotateLerpRoutine;
    private Coroutine flipLerpRoutine;
    public bool IsLerping => lerpRoutine != null;
    public bool IsRotateLerping => rotateLerpRoutine != null;
    public bool IsFlipLerping => flipLerpRoutine != null;
    private float targetRotation;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        CancelLerp();
        CancelRotateLerp();
    }

    #region Item Navigation

    public void HighlightItem(NavItem newItem)
    {
        currentItem = newItem;

        if (UIManager.Smoothing)
            HighlightItemLerp();
        else
            HighlightItemImmediate();
    }

    public void HighlightItemImmediate()
    {
        if (currentItem == null) return;

        GetWorldRectValues(currentItem.rect, out Vector2 targetPos, out Vector2 targetSize);

        rect.anchoredPosition = targetPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
    }

    private void HighlightItemLerp()
    {
        CancelLerp();

        lerpRoutine = StartCoroutine(LerpToRectTarget(
            getTarget: () =>
            {
                GetWorldRectValues(currentItem.rect, out var p, out var s);
                return (p, s);
            },
            shouldAbort: () => currentItem == null
        ));
    }

    #endregion

    #region Grid Navigation

    public void HighlightCell(Vector2Int cell, bool expanded = false)
    {
        currentItem = null;

        if (UIManager.Smoothing)
            HighlightCellLerp(cell, expanded);
        else
            HighlightCellImmediate(cell, expanded);

        if (!expanded)
            ResetRotation();
    }

    public void HighlightCellImmediate(Vector2Int cell, bool expanded = false)
    {
        GetCellRectValues(centerGridCell, cell, out var p, out var s);

        if (expanded) s *= 3;

        rect.anchoredPosition = p;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.y);
    }

    private void HighlightCellLerp(Vector2Int cell, bool expanded = false)
    {
        CancelLerp();

        lerpRoutine = StartCoroutine(LerpToRectTarget(
            getTarget: () =>
            {
                GetCellRectValues(centerGridCell, cell, out var p, out var s);
                if (expanded) s *= 3;
                return (p, s);
            },
            shouldAbort: () => currentItem != null
        ));
    }

    public void UpdateWithRectImmediate(RectTransform rt)
    {
        if (rt == null) return;

        GetWorldRectValues(rt, out Vector2 targetPos, out Vector2 targetSize);

        rect.anchoredPosition = targetPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
        rect.localEulerAngles = rt.localEulerAngles;
    }

    public Coroutine LerpWithRect(RectTransform rt)
    {
        CancelLerp();
        return lerpRoutine = StartCoroutine(LerpRect(rt));
    }

    private IEnumerator LerpRect(RectTransform rt)
    {
        if (rt == null) yield break;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 startSize = rect.sizeDelta;
        Vector2 startRot = rect.localEulerAngles;

        GetWorldRectValues(rt, out Vector2 targetPos, out Vector2 targetSize);

        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.unscaledDeltaTime;
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

    #endregion

    #region Rotate

    public void Rotate(bool cw)
    {
        float angle = cw ? -90 : 90;

        if (UIManager.Smoothing)
            RotateLerp(angle);
        else
            RotateImmediate(angle);
    }

    private void RotateImmediate(float angle)
    {
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localEulerAngles = new Vector3(0, 0, rect.localEulerAngles.z + angle);
    }

    private void RotateLerp(float angle)
    {
        targetRotation = rect.localEulerAngles.z + angle;

        CancelRotateLerp();

        rotateLerpRoutine = StartCoroutine(RotateRoutine(targetRotation));
    }

    private IEnumerator RotateRoutine(float finalAngle)
    {
        rect.pivot = new Vector2(0.5f, 0.5f);

        float start = rect.localEulerAngles.z;
        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.unscaledDeltaTime;
            float s = Mathf.SmoothStep(0, 1, t / transitionDuration);

            float newAngle = Mathf.LerpAngle(start, finalAngle, s);
            rect.localEulerAngles = new Vector3(0, 0, newAngle);

            yield return null;
        }

        rect.localEulerAngles = new Vector3(0, 0, finalAngle);
        rotateLerpRoutine = null;
    }

    public void ResetRotation()
    {
        targetRotation = 0;
        rect.localEulerAngles = Vector3.zero;
    }

    #endregion

    #region Flip

    public void Flip(bool horizontal)
    {
        if (UIManager.Smoothing)
            FlipLerp(horizontal);
        else
            FlipInstant(horizontal);
    }

    private void FlipInstant(bool horizontal)
    {
        Vector3 scale = rect.localScale;

        if (horizontal)
            scale.x *= -1f;
        else
            scale.y *= -1f;

        rect.localScale = scale;
    }

    private void FlipLerp(bool horizontal)
    {
        Vector3 targetScale = rect.localScale;

        if (horizontal)
            targetScale.x *= -1f;
        else
            targetScale.y *= -1f;

        CancelFlipLerp();

        flipLerpRoutine = StartCoroutine(FlipRoutine(targetScale));
    }

    private IEnumerator FlipRoutine(Vector3 targetScale)
    {
        Vector3 startScale = rect.localScale;

        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.unscaledDeltaTime;
            float s = Mathf.Clamp01(t / transitionDuration);
            s = Mathf.SmoothStep(0, 1, s);

            rect.localScale = Vector3.Lerp(startScale, targetScale, s);

            yield return null;
        }

        rect.localScale = targetScale;

        flipLerpRoutine = null;
    }

    #endregion

    #region Helpers

    public void CancelLerp()
    {
        if (IsLerping)
        {
            StopCoroutine(lerpRoutine);
            lerpRoutine = null;
        }
    }

    public void CancelRotateLerp()
    {
        if (IsRotateLerping)
        {
            StopCoroutine(rotateLerpRoutine);
            rotateLerpRoutine = null;
        }
    }

    public void CancelFlipLerp()
    {
        if(IsFlipLerping)
        {
            StopCoroutine(flipLerpRoutine);
            flipLerpRoutine = null;
        }
    }

    public IEnumerator WaitUntilDone()
    {
        while (IsLerping)
            yield return null;
    }

    private void GetWorldRectValues(RectTransform target, out Vector2 pos, out Vector2 size)
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

    private void GetCellRectValues(RectTransform target, Vector2Int cell, out Vector2 pos, out Vector2 size)
    {
        GetWorldRectValues(target, out var p, out var s);
        pos = new Vector2(p.x + cell.x * s.x, p.y + cell.y * s.y);
        size = s;
    }

    private IEnumerator LerpToRectTarget(System.Func<(Vector2 pos, Vector2 size)> getTarget, System.Func<bool> shouldAbort)
    {
        Vector2 startPos = rect.anchoredPosition;
        Vector2 startSize = rect.sizeDelta;

        float t = 0f;

        while (t < 1f)
        {
            if (shouldAbort())
            {
                CancelLerp();
                yield break;
            }

            var (targetPos, targetSize) = getTarget();

            t += Time.unscaledDeltaTime / transitionDuration;
            float s = Mathf.SmoothStep(0, 1, t);

            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, s);

            Vector2 newSize = Vector2.Lerp(startSize, targetSize, s);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);

            yield return null;
        }

        var (finalPos, finalSize) = getTarget();
        rect.anchoredPosition = finalPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalSize.y);

        lerpRoutine = null;
    }

    #endregion
}
