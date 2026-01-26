using System.Collections;
using UnityEngine;

public class GridCameraController : MonoBehaviour
{
    RectTransform rect;
    public RectTransform centerCellRt;
    public RectTransform target;

    private void OnEnable()
    {
        EventBus.Subscribe<NewGridCellEvent>(OnAdjustGridCameraEvent);
        EventBus.Subscribe<CancelCameraMovementEvent>(OnCancelCameraMovementEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NewGridCellEvent>(OnAdjustGridCameraEvent);
        EventBus.Unsubscribe<CancelCameraMovementEvent>(OnCancelCameraMovementEvent);
    }

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void OnAdjustGridCameraEvent(NewGridCellEvent e)
    {
        if (rect == null || target == null) return;
        MoveCameraSmooth();
    }

    void OnCancelCameraMovementEvent(CancelCameraMovementEvent e)
    {
        if (cameraMoveRoutine != null) StopCoroutine(cameraMoveRoutine);
    }

    private void MoveCameraSmooth()
    {
        if (cameraMoveRoutine != null) StopCoroutine(cameraMoveRoutine);
        cameraMoveRoutine = StartCoroutine(MoveCameraSmooth(0.5f));
    }

    Coroutine cameraMoveRoutine;
    IEnumerator MoveCameraSmooth(float duration)
    {
        float t = 0;

        Vector2 startPos = rect.localPosition;
        float scale = rect.localScale.x;
        Vector2 endPos = target.localPosition;
        endPos = new Vector2(-endPos.x * scale, -endPos.y * scale);

        while (t < duration)
        {
            endPos = target.localPosition;
            endPos = new Vector2(-endPos.x * scale, -endPos.y * scale);
            t += Time.deltaTime;
            float s = Mathf.Clamp01(t / duration);

            s = Mathf.SmoothStep(0, 1, s);

            SetPos(Vector3.Lerp(startPos, endPos, s));
            yield return null;
        }
        endPos = target.localPosition;
        endPos = new Vector2(-endPos.x * scale, -endPos.y * scale);
        SetPos(endPos);
    }

    public void SetPos(Vector2 pos)
    {
        rect.localPosition = pos;
    }
}
