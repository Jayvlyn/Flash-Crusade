using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class NavVisualizer : MonoBehaviour
{
    [Header("Settings")] 
    float transitionDuration = 0.12f;

    [HideInInspector] public RectTransform centerGridCell;
    [HideInInspector] public RectTransform rect;
    private NavItem currentItem;
    private Coroutine lerpRoutine;
    public bool IsLerping => lerpRoutine != null;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        CancelLerp();
    }

    public void OnHighlightItem(NavItem newItem)
    {
        currentItem = newItem;

        if (UIManager.Smoothing)
            LerpToCurrentItem();
        else
            UpdateCurrentItemImmediate();
    }

    public void OnHighlightGridCell(Vector2Int cell, bool expanded = false)
    {
        currentItem = null;

        if (UIManager.Smoothing)
            LerpToGridCell(cell, expanded);
        else
            UpdateGridCellImmediate(cell, expanded);
    }

    private void LerpToCurrentItem()
    {
        CancelLerp();

        lerpRoutine = StartCoroutine(LerpToTarget(
            getTarget: () =>
            {
                GetWorldRectValues(currentItem.rect, out var p, out var s);
                return (p, s);
            },
            shouldAbort: () => currentItem == null
        ));
    }
    
    private void LerpToGridCell(Vector2Int cell, bool expanded = false)
    {
        CancelLerp();

        lerpRoutine = StartCoroutine(LerpToTarget(
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

            yield return null;
        }

        rect.anchoredPosition = targetPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);

        lerpRoutine = null;
    }

    public void UpdateCurrentItemImmediate()
    {
        if (currentItem == null) return;

        GetWorldRectValues(currentItem.rect, out Vector2 targetPos, out Vector2 targetSize);

        rect.anchoredPosition = targetPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
    }

    public void UpdateGridCellImmediate(Vector2Int cell, bool expanded = false)
    {
        GetCellRectValues(centerGridCell, cell, out var p, out var s);

        if (expanded) s *= 3;

        rect.anchoredPosition = p;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.y);
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

    private void GetCellRectValues(RectTransform target, Vector2Int cell, out Vector2 pos, out Vector2 size)
    {
        GetWorldRectValues(target, out var p, out var s);
        pos = new Vector2(p.x + cell.x * s.x, p.y + cell.y * s.y);
        size = s;
    }

    private IEnumerator LerpToTarget(
        System.Func<(Vector2 pos, Vector2 size)> getTarget, 
        System.Func<bool> shouldAbort)
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

    public void CancelLerp()
    {
        if (IsLerping)
        {
            StopCoroutine(lerpRoutine);
            lerpRoutine = null;
        }
    }

    public IEnumerator WaitUntilDone()
    {
        while (IsLerping)
            yield return null;
    }
}
