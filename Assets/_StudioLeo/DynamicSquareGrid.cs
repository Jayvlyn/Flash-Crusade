using UnityEngine;
using UnityEngine.UI;

//[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class FixedRatioGrid : MonoBehaviour
{
    public int columns = 3;
    public int rows = 6;

    [Header("Layout Tuning")]
    [Range(0f, 0.5f)]
    public float spacingRatio = 0.05f;
    public float minimumPaddingH = 10f;
    public float minimumPaddingW = 10f;
    public float heightFactor = 0.01f;
    public float heightFactor2 = 0.01f;
    public float cellReduction = 0.01f;

    private RectTransform rectTransform;
    private GridLayoutGroup grid;
    private Vector2 lastSize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        grid = GetComponent<GridLayoutGroup>();
    }

    //void Update()
    private void Start()
    {
        if (rectTransform.rect.size != lastSize)
        {
            lastSize = rectTransform.rect.size;
            Recalculate();
        }
    }

    public void Recalculate()
    {
        float w = rectTransform.rect.width;
        float h = rectTransform.rect.height;

        float aspectGrid = (float)columns / rows;
        float aspectPanel = w / h;

        bool heightLimited = aspectPanel > aspectGrid;

        float rawCellSize = heightLimited ? h / rows : w / columns;

        float spacing = rawCellSize * spacingRatio;
        grid.spacing = new Vector2(spacing, spacing);

        float totalSpacingX = spacing * (columns - 1);
        float totalSpacingY = spacing * (rows - 1);

        float availableW = w;
        float availableH = h;

        float cellSize = heightLimited
            ? (availableH - totalSpacingY) / rows
            : (availableW - totalSpacingX) / columns;

        float usedWidth = cellSize * columns + totalSpacingX;
        float usedHeight = cellSize * rows + totalSpacingY;

        float leftoverW = availableW - usedWidth;
        float leftoverH = availableH - usedHeight;

        int padLeft = Mathf.RoundToInt(leftoverW * 0.5f);
        int padRight = padLeft;
        int padTop = heightLimited
            ? Mathf.RoundToInt(leftoverH * 0.5f) + (int)(h * heightFactor)
            : Mathf.RoundToInt(leftoverH * 0.5f) + (int)(h * heightFactor2);
        int padBottom = padTop;

        padLeft = Mathf.Max(padLeft, (int)minimumPaddingW);
        padRight = Mathf.Max(padRight, (int)minimumPaddingW);
        //padTop = Mathf.Max(padTop, (int)minimumPadding);
        padTop = heightLimited
            ? Mathf.Max(padTop, (int)minimumPaddingH) + (int)(h * heightFactor)
            : Mathf.Max(padTop, (int)minimumPaddingH) + (int)(h * heightFactor2);
        padBottom = Mathf.Max(padBottom, (int)minimumPaddingH);

        float effectiveW = w - padLeft - padRight - totalSpacingX;
        float effectiveH = h - padTop - padBottom - totalSpacingY;

        cellSize = heightLimited
            ? effectiveH / rows - cellReduction
            : effectiveW / columns - cellReduction;

        grid.cellSize = new Vector2(cellSize, cellSize);
        grid.padding = new RectOffset(padLeft, padRight, padTop, 0);
    }
}
