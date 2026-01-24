using System.Collections;
using UnityEngine;

public class GridCameraController : MonoBehaviour
{
    RectTransform rect;
    public RectTransform centerCellRt;

    private void OnEnable()
    {
        EventBus.Subscribe<NewGridCellEvent>(OnNewGridCellEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NewGridCellEvent>(OnNewGridCellEvent);
    }

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void OnNewGridCellEvent(NewGridCellEvent e)
    {
        if (rect == null) return;
        Vector2 pos = rect.localPosition;

        Vector3[] corners = new Vector3[4];
        centerCellRt.GetWorldCorners(corners);

        Vector2 bl = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 tr = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        float pixels = tr.x - bl.x;
        pos.x = e.cell.x * pixels;
        pos.y = e.cell.y * pixels;
        MoveCameraSmooth(-pos);
    }

    private void MoveCameraSmooth(Vector2 pos)
    {
        if(cameraMoveRoutine!=null) StopCoroutine(cameraMoveRoutine);
        cameraMoveRoutine = StartCoroutine(MoveCameraSmooth(pos, 0.5f));
    }

    Coroutine cameraMoveRoutine;
    IEnumerator MoveCameraSmooth(Vector2 pos, float duration)
    {
        Vector2 startPos = rect.localPosition;
        Vector2 targetPos = pos;

        float t = 0;

        while(t < duration)
        {
            t += Time.deltaTime;
            float s = Mathf.Clamp01(t/duration);

            s = Mathf.SmoothStep(0, 1, s);

            rect.localPosition = Vector3.Lerp(startPos, targetPos, s);
            yield return null;
        }

        rect.localPosition = targetPos;
    }
}
