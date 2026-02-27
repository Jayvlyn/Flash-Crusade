using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class EditorNavVisualizer : NavVisualizer, IEditorNavVisualizer
{
    [HideInInspector] public RectTransform centerGridCell;

    float targetRotation;
    bool expanded;

    public bool IsRotateLerping => rotateLerpRoutine != null;
    public bool IsFlipLerping => flipLerpRoutine != null;

    Coroutine rotateLerpRoutine;
    Coroutine flipLerpRoutine;

    #region Lifecycle

    private void OnEnable()
    {
        EventBus.Subscribe<TabSizeUpdatedEvent>(OnTabSizeUpdatedEvent);
    }

    private void OnDisable()
    {
        CancelLerp();
        CancelRotateLerp();
        EventBus.Unsubscribe<TabSizeUpdatedEvent>(OnTabSizeUpdatedEvent);
    }

    #endregion

    #region IVisualizer

    public void HighlightCellImmediate(Vector2Int cell)
    {
        GetCellRectValues(centerGridCell, cell, out var p, out var s);

        if (expanded) s *= 3;

        rect.anchoredPosition = p;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.y);
    }

    public void SetExpanded(bool expanded) => this.expanded = expanded;

    #endregion

    #region Item Navigation

    private void OnTabSizeUpdatedEvent(TabSizeUpdatedEvent e) => HighlightItemImmediate();

    #endregion

    #region Grid Navigation

    public void HighlightCell(Vector2Int cell)
    {
        if (UIManager.Smoothing)
            HighlightCellLerp(cell);
        else
            HighlightCellImmediate(cell);

        if (!expanded)
            ResetRotation();
    }

    private void HighlightCellLerp(Vector2Int cell)
    {
        CancelLerp();

        lerpRoutine = StartCoroutine(LerpToRectTarget(
            getTarget: () =>
            {
                GetCellRectValues(centerGridCell, cell, out var p, out var s);
                if (expanded)
                    s *= 3;
                return (p, s);
            },
            shouldAbort: () => false
        ));
    }

    #endregion

    #region Rotate

    public void Rotate(float angle)
    {
        if (UIManager.Smoothing)
            RotateLerp(angle);
        else
            RotateImmediate(angle);
    }

    public void RotateImmediate(float angle)
    {
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localEulerAngles = new Vector3(0, 0, rect.localEulerAngles.z - angle);
    }

    private void RotateLerp(float angle)
    {
        targetRotation = rect.localEulerAngles.z - angle;

        CancelRotateLerp();

        rotateLerpRoutine = StartCoroutine(RotateRoutine(targetRotation));
    }

    private IEnumerator RotateRoutine(float finalAngle)
    {
        rect.pivot = new Vector2(0.5f, 0.5f);

        float startAngle = rect.localEulerAngles.z;
        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0, 1, t / transitionDuration);

            float newAngle = Mathf.LerpAngle(startAngle, finalAngle, s);
            rect.localEulerAngles = new Vector3(0, 0, newAngle);

            yield return null;
        }

        rect.localEulerAngles = new Vector3(0, 0, Mathf.RoundToInt(finalAngle));
        rotateLerpRoutine = null;
    }

    public void ResetRotation()
    {
        targetRotation = 0;
        rect.localEulerAngles = Vector3.zero;
    }

    #endregion

    #region Flip

    public void Flip(FlipAxis axis)
    {
        if (UIManager.Smoothing)
            FlipLerp(axis);
        else
            FlipImmediate(axis);
    }

    public void FlipImmediate(FlipAxis axis)
    {
        Vector3 scale = rect.localScale;

        if (axis == FlipAxis.Horizontal)
            scale.x *= -1f;
        else
            scale.y *= -1f;

        rect.localScale = scale;
    }

    private void FlipLerp(FlipAxis axis)
    {
        Vector3 targetScale = rect.localScale;

        if (axis == FlipAxis.Horizontal)
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
            t += Time.deltaTime;
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

    private void GetCellRectValues(RectTransform target, Vector2Int cell, out Vector2 pos, out Vector2 size)
    {
        GetWorldRectValues(target, out var p, out var s);
        pos = new Vector2(p.x + cell.x * s.x, p.y + cell.y * s.y);
        size = s;
    }

    #endregion
}
