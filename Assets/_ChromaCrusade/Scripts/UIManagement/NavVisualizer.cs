using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class NavVisualizer : MonoBehaviour
{
    RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnHighlightNew(NavItem newItem)
    {
        MatchWorldRect(newItem.rect);
    }

    public void MatchWorldRect(RectTransform target)
    {
        // Get target world corners
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        // Convert corners to this rect's parent space
        RectTransform parent = rect.parent as RectTransform;
        Vector3[] localCorners = new Vector3[4];

        for (int i = 0; i < 4; i++)
            localCorners[i] = parent.InverseTransformPoint(corners[i]);

        // Compute new anchored position + size
        Vector3 bottomLeft = localCorners[0];
        Vector3 topRight = localCorners[2];

        Vector2 newSize = topRight - bottomLeft;
        Vector2 newPos = (Vector2)bottomLeft + newSize * 0.5f;

        // Apply
        rect.anchoredPosition = newPos;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
    }
}
