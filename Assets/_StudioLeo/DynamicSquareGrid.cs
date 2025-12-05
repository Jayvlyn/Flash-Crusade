using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class FixedRatioGrid : MonoBehaviour
{
    public int columns = 3;
    public int rows = 6;

    [Header("Layout Tuning")]
    [Range(0f, 0.5f)]
    public float spacingRatio = 0.05f;
    public float minimumPadding = 10f; // pixels
    public float cellReduction = 0.01f;

    private RectTransform rectTransform;
    private GridLayoutGroup grid;
    private Vector2 lastSize;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        grid = GetComponent<GridLayoutGroup>();
    }

    void Update()
    //void Start()
    {
        if (rectTransform.rect.size != lastSize)
        {
            lastSize = rectTransform.rect.size;
            Recalculate();
        }
    }

    void Recalculate()
    {
        float w = rectTransform.rect.width;
        float h = rectTransform.rect.height;

        float aspectGrid = (float)columns / rows;
        float aspectPanel = w / h;

        // First determine limiting axis
        bool heightLimited = aspectPanel > aspectGrid;

        // Determine a provisional cell size ignoring padding
        float rawCellSize = heightLimited ? h / rows : w / columns;

        // Calculate spacing based on cell scale
        float spacing = rawCellSize * spacingRatio;
        grid.spacing = new Vector2(spacing, spacing);

        float totalSpacingX = spacing * (columns - 1);
        float totalSpacingY = spacing * (rows - 1);

        float availableW = w;
        float availableH = h;

        // Padding applied later will reduce this space
        float cellSize = heightLimited
            ? (availableH - totalSpacingY) / rows
            : (availableW - totalSpacingX) / columns;

        // Now compute padding leftover for both axes
        float usedWidth = cellSize * columns + totalSpacingX;
        float usedHeight = cellSize * rows + totalSpacingY;

        float leftoverW = availableW - usedWidth;
        float leftoverH = availableH - usedHeight;

        // Compute padding evenly
        int padLeft = Mathf.RoundToInt(leftoverW * 0.5f);
        int padRight = padLeft;
        int padTop = Mathf.RoundToInt(leftoverH * 0.5f);
        int padBottom = padTop;

        // **Clamp padding on all sides**
        padLeft = Mathf.Max(padLeft, (int)minimumPadding);
        padRight = Mathf.Max(padRight, (int)minimumPadding);
        padTop = Mathf.Max(padTop, (int)minimumPadding);
        padBottom = Mathf.Max(padBottom, (int)minimumPadding);

        // Now that padding changed, we must recompute cell size properly:
        float effectiveW = w - padLeft - padRight - totalSpacingX;
        float effectiveH = h - padTop - padBottom - totalSpacingY;

        cellSize = heightLimited
            ? effectiveH / rows - cellReduction
            : effectiveW / columns - cellReduction;

        // Assign final values
        grid.cellSize = new Vector2(cellSize, cellSize);
        grid.padding = new RectOffset(padLeft, padRight, padTop, padBottom);
    }
}
