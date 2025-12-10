using System.Collections.Generic;
using UnityEngine;

public class EditorBuildArea : MonoBehaviour
{
    public Dictionary<Vector2Int, EditorShipPart> occupiedCells = new Dictionary<Vector2Int, EditorShipPart>();
    public RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public bool CanPlacePart(Vector2Int centerCell, EditorShipPart part)
    {
        return CellsAvailable(centerCell, part);
    }

    public EditorShipPart GetPartAtCell(Vector2Int cell)
    {
        if (occupiedCells.ContainsKey(cell)) return occupiedCells[cell];
        else return null;
    }

    public bool PlacePart(Vector2Int centerCell, EditorShipPart part)
    {
        if(!CellsAvailable(centerCell, part)) return false;

        ForEachSegment(part, centerCell, cell =>
        {
            part.cellPlacedAt = cell;
            occupiedCells[cell] = part;
            return true; // keep iterating
        });

        return true;
    }

    public EditorShipPart GrabPart(Vector2Int cell)
    {
        EditorShipPart partAtCell = GetPartAtCell(cell);
        if (partAtCell != null)
        {
            ForEachSegment(partAtCell, partAtCell.position, cell =>
            {
                occupiedCells.Remove(cell);
                return true; // keep iterating
            });
            return partAtCell;
        }
        return null; // no part to grab here
    }

    public bool CellsAvailable(Vector2Int centerCell, EditorShipPart part)
    {
        return ForEachSegment(part, centerCell, cell =>
            !occupiedCells.ContainsKey(cell)
        );
    }

    private bool ForEachSegment(EditorShipPart part, Vector2Int centerCell, System.Func<Vector2Int, bool> callback)
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                var segment = part.segments[x, y];
                if (segment == null || segment.segmentState == SegmentState.Disabled)
                    continue;

                int xOffset = part.xFlipped ? 1 - x : x - 1;
                int yOffset = part.yFlipped ? y - 1 : 1 - y;
                Vector2Int offset = new Vector2Int(xOffset, yOffset);

                Vector2Int rotatedOffset = part.Rotation switch
                {
                    0 => offset,
                    90 => new Vector2Int(offset.y, -offset.x),
                    180 => new Vector2Int(-offset.x, -offset.y),
                    270 => new Vector2Int(-offset.y, offset.x),
                    _ => offset
                };

                Vector2Int targetCell = centerCell + rotatedOffset;

                if (!callback(targetCell)) return false;
            }
        }
        return true;
    }
}
