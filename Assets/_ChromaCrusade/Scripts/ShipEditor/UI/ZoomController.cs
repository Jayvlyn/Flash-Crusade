using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Belongs on the Build Area rect that is parent to the grid and parts
/// </summary>
public class ZoomController : MonoBehaviour
{
    [SerializeField] Vector2 zoomRange = new Vector2(1, 10);

    Dictionary<int, float> zoomScales = new Dictionary<int, float>();

    int zoomLevel = 3;
    public int ZoomLevel
    {
        get => zoomLevel;
        set => zoomLevel = (int)Mathf.Clamp(value, zoomRange.x, zoomRange.y);
    }
    #region Lifecycle

    void OnEnable()
    {
        EventBus.Subscribe<ZoomInputEvent>(OnZoomInputEvent);
        EventBus.Subscribe<NewGridCellEvent>(OnNewGridCellEvent);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<ZoomInputEvent>(OnZoomInputEvent);
        EventBus.Unsubscribe<NewGridCellEvent>(OnNewGridCellEvent);
    }

    void Awake()
    {
        InitZoomScales();
    }

    #endregion

    void InitZoomScales()
    {
        // based on 16:9 ratio
        zoomScales.Add(1, 1.48f);
        zoomScales.Add(2, 0.89f);
        zoomScales.Add(3, 0.635f);
        zoomScales.Add(4, 0.493f);
        zoomScales.Add(5, 0.403f);
        zoomScales.Add(6, 0.3415f);
        zoomScales.Add(7, 0.29597f);
        zoomScales.Add(8, 0.26114f);
        zoomScales.Add(9, 0.23365f);
        zoomScales.Add(10, 0.21141f);
    }

    Coroutine zoomRoutine;
    Vector3 targetZoomScale;
    void OnNewZoomLevel()
    {
        float s = zoomScales[zoomLevel];
        targetZoomScale = new Vector3(s, s, s);

        if (UIManager.Smoothing)
        {
            if (zoomRoutine != null) StopCoroutine(zoomRoutine);
            zoomRoutine = StartCoroutine(LerpZoom(targetZoomScale));
        }
        else
        {
            transform.localScale = new Vector3(s, s, s);
            EventBus.Publish(new NewZoomLevelEvent());
        }
    }

    public static bool MidZoom;
    IEnumerator LerpZoom(Vector3 target, float duration = 0.15f)
    {
        MidZoom = true;
        float t = 0f;
        Vector3 start = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float s = Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = Vector3.Lerp(start, target, t);
            EventBus.Publish(new NewZoomLevelEvent());

            yield return null;
        }
        transform.localScale = target;
        MidZoom = false;
    }

    void OnZoomInputEvent(ZoomInputEvent e)
    {
        ZoomDirection zoomDir = e.zoomDirection;
        if (zoomDir == ZoomDirection.In) ZoomLevel--;
        else if (zoomDir == ZoomDirection.Out) ZoomLevel++;
        OnNewZoomLevel();
    }

    void OnNewGridCellEvent(NewGridCellEvent e)
    {
        //if(delayedPivotChange != null) StopCoroutine(delayedPivotChange);
        //delayedPivotChange = StartCoroutine(DelayedPivotChange(e));
    }

    Coroutine delayedPivotChange;
    IEnumerator DelayedPivotChange(NewGridCellEvent e)
    {
        yield return new WaitForSeconds(1f);

        RectTransform rect = (RectTransform)transform;

        Vector2 oldPivot = rect.pivot;
        Vector2 size = rect.rect.size;
        Vector3 scale = rect.localScale;

        Vector2 cellOffsetPx = new Vector2(e.cell.x * 25f, e.cell.y * 25f);

        Vector2 newPivot;
        newPivot.x = 0.5f + (cellOffsetPx.x / size.x);
        newPivot.y = 0.5f + (cellOffsetPx.y / size.y);

        Vector2 pivotDelta = newPivot - oldPivot;

        Vector3 positionDelta = new Vector3(
            pivotDelta.x * size.x * scale.x,
            pivotDelta.y * size.y * scale.y,
            0f
        );

        rect.localPosition += positionDelta;
        rect.pivot = newPivot;
    }

}
