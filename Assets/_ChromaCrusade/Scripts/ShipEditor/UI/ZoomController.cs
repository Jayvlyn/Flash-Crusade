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
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<ZoomInputEvent>(OnZoomInputEvent);
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
            transform.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
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
}
