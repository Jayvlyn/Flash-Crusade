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

    public EditorShipPart GetPartAtCell(Vector2Int cell)
    {
        if (occupiedCells.ContainsKey(cell)) return occupiedCells[cell];
        else return null;
    }

    public bool PlacePart(Vector2Int centerCell, EditorShipPart part)
    {
        Debug.Log("Trying to place part");
        if(!CellsAvailable(centerCell, part)) return false;
        Debug.Log("Cells available");

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                var segment = part.segments[x, y];
                if (segment == null)
                    continue;

                // offset relative to center
                int offsetX = x - 1;
                int offsetY = y - 1;

                Vector2Int targetCell = new Vector2Int(
                    centerCell.x + offsetX,
                    centerCell.y + offsetY
                );

                occupiedCells.Add(targetCell, part);
                Debug.Log("added at " + targetCell);
            }
        }

        return true;
    }

    public EditorShipPart GrabPart(Vector2Int cell)
    {
        EditorShipPart partAtCell = GetPartAtCell(cell);
        if (partAtCell != null)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    var segment = partAtCell.segments[x, y];
                    if (segment == null)
                        continue;

                    // offset relative to center
                    int offsetX = x - 1;
                    int offsetY = y - 1;

                    Vector2Int targetCell = new Vector2Int(
                        cell.x + offsetX,
                        cell.y + offsetY
                    );

                    occupiedCells.Remove(targetCell);
                }
            }
            return partAtCell;
        }
        return null; // no part to grab here
    }

    public bool CellsAvailable(Vector2Int centerCell, EditorShipPart part)
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                var segment = part.segments[x, y];
                if (segment == null)
                    continue;

                // offset relative to center
                int offsetX = x - 1;
                int offsetY = y - 1;

                Vector2Int targetCell = new Vector2Int(
                    centerCell.x + offsetX,
                    centerCell.y + offsetY
                );

                if (occupiedCells.ContainsKey(targetCell))
                    return false;
            }
        }
        return true;
    }


}
