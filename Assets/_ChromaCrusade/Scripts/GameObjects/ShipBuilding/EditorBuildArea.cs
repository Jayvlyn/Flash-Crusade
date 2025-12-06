using System.Collections.Generic;
using UnityEngine;

public class EditorBuildArea : MonoBehaviour
{
    public Dictionary<Vector2Int, EditorShipPart> occupiedCells = new Dictionary<Vector2Int, EditorShipPart>();

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

        int i = 0;

        foreach (var segment in part.segments)
        {
            if (segment != null)
            {
                Vector2Int targetCell = centerCell + EditorShipPart.offsets[i];
                occupiedCells.Add(targetCell, part);
                Debug.Log("Segment added at " + targetCell.ToString());
            }

            i++;
        }

        Debug.Log("Placed Fully");

        return true;
    }

    public EditorShipPart GrabPart(Vector2Int cell)
    {
        EditorShipPart partAtCell = GetPartAtCell(cell);
        if (partAtCell != null)
        {
            int i = 0;

            foreach (var segment in partAtCell.segments)
            {
                if (segment != null)
                {
                    Vector2Int targetCell = cell + EditorShipPart.offsets[i];
                    occupiedCells.Remove(targetCell);
                }

                i++;
            }
            return partAtCell; // successfully removed segents from dict
        }
        return null; // no part to grab here
    }

    public bool CellsAvailable(Vector2Int centerCell, EditorShipPart part)
    {
        int i = 0;

        foreach (var segment in part.segments)
        {
            if (segment != null)
            {
                Vector2Int targetCell = centerCell + EditorShipPart.offsets[i];
                if (occupiedCells.ContainsKey(targetCell))
                    return false;
            }

            i++;
        }

        return true;
    }
}
