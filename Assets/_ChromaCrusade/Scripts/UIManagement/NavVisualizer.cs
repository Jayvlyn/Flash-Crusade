using UnityEngine;
using System.Collections;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(RectTransform))]
public class NavVisualizer : MonoBehaviour
{
    public float transitionDuration = 0.12f;
    public float cellSize = 32;

    [HideInInspector] public RectTransform centerGridCell;
    private RectTransform rect;
    private NavItem currentItem;
    private Coroutine lerpRoutine;

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

    public void OnHighlightGridCell(Vector3Int cell)
    {
        currentItem = null;

        if (UIManager.Smoothing)
            LerpToGridCell(cell);
        else
            UpdateGridCellImmediate(cell);
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
    
    private void LerpToGridCell(Vector3Int cell)
    {
        CancelLerp();

        lerpRoutine = StartCoroutine(LerpToTarget(
            getTarget: () =>
            {
                GetCellRectValues(centerGridCell, cell, out var p, out var s);
                return (p, s);
            },
            shouldAbort: () => currentItem != null
        ));
    }

    public void UpdateCurrentItemImmediate()
    {
        if (currentItem == null) return;

        GetWorldRectValues(currentItem.rect, out Vector2 targetPos, out Vector2 targetSize);

        rect.anchoredPosition = targetPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
    }

    private void UpdateGridCellImmediate(Vector3Int cell)
    {
        GetCellRectValues(centerGridCell, cell, out var p, out var s);

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

    private void GetCellRectValues(RectTransform target, Vector3Int cell, out Vector2 pos, out Vector2 size)
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
        if (lerpRoutine != null)
        {
            StopCoroutine(lerpRoutine);
            lerpRoutine = null;
        }
    }
}
