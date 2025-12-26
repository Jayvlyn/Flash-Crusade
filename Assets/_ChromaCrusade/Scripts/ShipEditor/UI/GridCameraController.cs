using UnityEngine;

public class GridCameraController : MonoBehaviour
{
    RectTransform rect;
    float pixelsPerCell = 16;

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
        Debug.Log(e.cell);
        Vector2 pos = rect.localPosition;
        pos.x = e.cell.x * pixelsPerCell;
        pos.y = e.cell.y * pixelsPerCell;
        rect.localPosition = -pos;
    }
}
