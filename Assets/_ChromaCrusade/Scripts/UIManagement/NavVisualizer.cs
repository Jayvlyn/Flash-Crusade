using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class NavVisualizer : MonoBehaviour
{
    public float transitionDuration = 0.12f;

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

    public void OnHighlightGridCell(Grid grid, Vector3Int cell)
    {
        currentItem = null;

        if (UIManager.Smoothing)
            LerpToGridCell(grid, cell);
        else
            UpdateGridCellImmediate(grid, cell);
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
    
    private void LerpToGridCell(Grid grid, Vector3Int cell)
    {
        CancelLerp();

        lerpRoutine = StartCoroutine(LerpToTarget(
            getTarget: () =>
            {
                GetGridCellRect(grid, cell, out var p, out var s);
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

    private void UpdateGridCellImmediate(Grid grid, Vector3Int cell)
    {
        GetGridCellRect(grid, cell, out Vector2 pos, out Vector2 size);

        rect.anchoredPosition = pos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
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

    private void GetGridCellRect(Grid grid, Vector3Int cell, out Vector2 pos, out Vector2 size)
    {
        Vector3 p0 = grid.CellToWorld(cell);
        Vector3 p1 = grid.CellToWorld(cell + new Vector3Int(1, 0, 0));
        Vector3 p2 = grid.CellToWorld(cell + new Vector3Int(1, 1, 0));
        Vector3 p3 = grid.CellToWorld(cell + new Vector3Int(0, 1, 0));

        Vector3[] worldCorners = { p0, p1, p2, p3 };
        Vector2[] canvasLocal = new Vector2[4];

        Canvas canvas = rect.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.transform as RectTransform;

        for (int i = 0; i < 4; i++)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldCorners[i]);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out canvasLocal[i]
            );
        }

        Vector2 bl = canvasLocal[0];
        Vector2 tr = canvasLocal[2];

        size = tr - bl;
        pos = bl + size * 0.5f;
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
