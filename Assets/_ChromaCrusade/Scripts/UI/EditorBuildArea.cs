using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EditorShipPart;

public class EditorBuildArea : MonoBehaviour
{
    public Dictionary<Vector2Int, EditorShipPart> occupiedCells = new Dictionary<Vector2Int, EditorShipPart>();
    [HideInInspector] public RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    #region Public API

    public bool CanPlacePart(EditorShipPart part, Vector2Int centerCell)
    {
        return CellsAvailable(centerCell, part);
    }

    public EditorShipPart GetPartAtCell(Vector2Int cell)
    {
        if (occupiedCells.ContainsKey(cell)) return occupiedCells[cell];
        else return null;
    }

    public bool PlacePart(EditorShipPart part, Vector2Int centerCell)
    {
        if (!CellsAvailable(centerCell, part))
            return false;

        RegisterPart(part);

        ForEachSegment(part, centerCell, (segment, cell) =>
        {
            part.cellPlacedAt = cell;
            occupiedCells[cell] = part;

            if (cell == Vector2Int.zero)
                centerPart = part;

            TryConnectSegment(part, segment, cell);
            return true;
        });

        bool connected = CanReachCenter(part);

        if (connected)
        {
            part.PartConnected();
            PropagateConnectedState(part);
        }
        else
        {
            part.PartDisconnected();
        }

        return true;
    }

    public EditorShipPart GrabPart(Vector2Int cell)
    {
        EditorShipPart partAtCell = GetPartAtCell(cell);
        if (partAtCell == null)
            return null;

        List<EditorShipPart> neighbors = adjacency.ContainsKey(partAtCell)
            ? adjacency[partAtCell].ToList()
            : new List<EditorShipPart>();

        ForEachSegment(partAtCell, partAtCell.position, (segment, c) =>
        {
            occupiedCells.Remove(c);
            return true;
        });

        RemoveNodeFromGraph(partAtCell);

        allParts.Remove(partAtCell);

        if (partAtCell == centerPart) centerPart = null;

        foreach (var n in neighbors)
            CheckAndPropagateDisconnect(n);

        return partAtCell;
    }

    #region Old Grab & Place (Full Recompute)

    //public EditorShipPart GrabPart(Vector2Int cell)
    //{
    //    EditorShipPart partAtCell = GetPartAtCell(cell);
    //    if (partAtCell == null)
    //        return null;

    //    ForEachSegment(partAtCell, partAtCell.position, (segment, segCell) =>
    //    {
    //        occupiedCells.Remove(segCell);
    //        return true;
    //    });

    //    RemoveNodeFromGraph(partAtCell);
    //    allParts.Remove(partAtCell);

    //    if (partAtCell == centerPart) centerPart = null;

    //    RecomputeConnectivity();

    //    return partAtCell;
    //}


    //public bool PlacePart(Vector2Int centerCell, EditorShipPart part)
    //{
    //    if (!CellsAvailable(centerCell, part))
    //        return false;

    //    ForEachSegment(part, centerCell, (segment, cell) =>
    //    {
    //        part.cellPlacedAt = cell;
    //        occupiedCells[cell] = part;

    //        if (cell == Vector2Int.zero)
    //            centerPart = part;

    //        TryConnectSegment(part, segment, cell);

    //        return true;
    //    });

    //    allParts.Add(part);
    //    RecomputeConnectivity();

    //    return true;
    //}

    #endregion

    #endregion

    #region Spacial Helpers

    private bool ForEachSegment(EditorShipPart part, Vector2Int centerCell, System.Func<EditorPartSegment, Vector2Int, bool> callback)
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

                if (!callback(segment, targetCell)) return false;
            }
        }
        return true;
    }

    private Vector2Int TransformDirection(Vector2Int dir, EditorShipPart part)
    {
        Vector2Int d = dir;

        if (part.xFlipped) d.x *= -1;
        if (part.yFlipped) d.y *= -1;

        return part.Rotation switch
        {
            0 => d,
            90 => new Vector2Int(d.y, -d.x),
            180 => new Vector2Int(-d.x, -d.y),
            270 => new Vector2Int(-d.y, d.x),
            _ => d
        };
    }

    private Vector2Int InverseTransformDirection(Vector2Int dir, EditorShipPart part)
    {
        Vector2Int d = dir;

        d = part.Rotation switch
        {
            0 => d,
            90 => new Vector2Int(-d.y, d.x),
            180 => new Vector2Int(-d.x, -d.y),
            270 => new Vector2Int(d.y, -d.x),
            _ => d
        };

        if (part.xFlipped) d.x *= -1;
        if (part.yFlipped) d.y *= -1;

        return d;
    }

    private EditorPartSegment GetSegmentAtCell(EditorShipPart part, Vector2Int cell)
    {
        Vector2Int offset = cell - part.position;

        Vector2Int unrotated = part.Rotation switch
        {
            0 => offset,
            90 => new Vector2Int(-offset.y, offset.x),
            180 => new Vector2Int(-offset.x, -offset.y),
            270 => new Vector2Int(offset.y, -offset.x),
            _ => offset
        };

        if (part.xFlipped) unrotated.x *= -1;
        if (part.yFlipped) unrotated.y *= -1;


        int segX = unrotated.x + 1;
        int segY = 1 - unrotated.y;

        if (segX < 0 || segX > 2 || segY < 0 || segY > 2)
            return null;

        EditorPartSegment segment = part.segments[segX, segY];

        if (segment == null || segment.segmentState == SegmentState.Disabled)
            return null;

        return segment;
    }


    private bool CellsAvailable(Vector2Int centerCell, EditorShipPart part)
    {
        return ForEachSegment(part, centerCell, (segment, cell) =>
            !occupiedCells.ContainsKey(cell)
        );
    }

    #endregion

    #region Graph

    private Dictionary<EditorShipPart, List<EditorShipPart>> adjacency = new Dictionary<EditorShipPart, List<EditorShipPart>>();
    private HashSet<EditorShipPart> allParts = new HashSet<EditorShipPart>();

    private void AddEdge(EditorShipPart a, EditorShipPart b)
    {
        if (!adjacency.ContainsKey(a)) adjacency[a] = new List<EditorShipPart>();
        if (!adjacency.ContainsKey(b)) adjacency[b] = new List<EditorShipPart>();

        if (!adjacency[a].Contains(b)) adjacency[a].Add(b);
        if (!adjacency[b].Contains(a)) adjacency[b].Add(a);
    }

    private void RemoveNodeFromGraph(EditorShipPart part)
    {
        if (!adjacency.ContainsKey(part)) return;

        foreach (var neighbor in adjacency[part])
        {
            adjacency[neighbor].Remove(part);
        }

        adjacency.Remove(part);
    }

    private void FindConnectingNeighbor(Vector2Int cell, Vector2Int dir, EditorShipPart part)
    {
        Vector2Int connectingCell = cell + dir;

        EditorShipPart neighbor = GetPartAtCell(connectingCell);
        if (neighbor != null)
        {
            if (NeighborConnectsBack(neighbor, connectingCell, cell))
            {
                AddEdge(part, neighbor);
            }
        }
    }

    private bool NeighborConnectsBack(EditorShipPart neighbor, Vector2Int neighborCell, Vector2Int thisCell)
    {
        Vector2Int worldDir = thisCell - neighborCell;

        EditorPartSegment seg = GetSegmentAtCell(neighbor, neighborCell);
        if (seg == null)
            return false;

        Vector2Int localDir = InverseTransformDirection(worldDir, neighbor);

        if (localDir == Vector2Int.up)
            return seg.topConnection.connectionState == ConnectionState.Enabled;

        if (localDir == Vector2Int.down)
            return seg.bottomConnection.connectionState == ConnectionState.Enabled;

        if (localDir == Vector2Int.left)
            return seg.leftConnection.connectionState == ConnectionState.Enabled;

        if (localDir == Vector2Int.right)
            return seg.rightConnection.connectionState == ConnectionState.Enabled;

        return false;
    }

    private void RegisterPart(EditorShipPart part)
    {
        allParts.Add(part);
        if (!adjacency.ContainsKey(part))
            adjacency[part] = new List<EditorShipPart>();
    }

    private void TryConnectSegment(EditorShipPart part, EditorPartSegment segment, Vector2Int cell)
    {
        if (segment.topConnection.connectionState == ConnectionState.Enabled)
            FindConnectingNeighbor(cell, TransformDirection(Vector2Int.up, part), part);

        if (segment.bottomConnection.connectionState == ConnectionState.Enabled)
            FindConnectingNeighbor(cell, TransformDirection(Vector2Int.down, part), part);

        if (segment.leftConnection.connectionState == ConnectionState.Enabled)
            FindConnectingNeighbor(cell, TransformDirection(Vector2Int.left, part), part);

        if (segment.rightConnection.connectionState == ConnectionState.Enabled)
            FindConnectingNeighbor(cell, TransformDirection(Vector2Int.right, part), part);
    }

    #endregion

    #region Connectivity

    private EditorShipPart centerPart;

    private bool CanReachCenter(EditorShipPart start)
    {
        Queue<EditorShipPart> queue = new Queue<EditorShipPart>();
        HashSet<EditorShipPart> visited = new HashSet<EditorShipPart>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == centerPart)
                return true;

            foreach (var neighbor in adjacency[current])
            {
                if (!visited.Contains(neighbor) && neighbor.partState == PartState.PlacedConnected)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    private void CheckAndPropagateDisconnect(EditorShipPart startPart)
    {
        if (startPart == centerPart) return;

        if (startPart.partState == PartState.PlacedDisconnected) return;

        if (CanReachCenter(startPart)) return;

        startPart.PartDisconnected();

        foreach (var n in adjacency[startPart]) CheckAndPropagateDisconnect(n);
    }

    private void PropagateConnectedState(EditorShipPart part)
    {
        if (!adjacency.ContainsKey(part)) return;

        Queue<EditorShipPart> queue = new Queue<EditorShipPart>();
        HashSet<EditorShipPart> visited = new HashSet<EditorShipPart>();

        queue.Enqueue(part);
        visited.Add(part);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var neighbor in adjacency[current])
            {
                if (!visited.Contains(neighbor) && neighbor.partState == PartState.PlacedDisconnected)
                {
                    neighbor.PartConnected();
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private void RecomputeConnectivity()
    {
        if (centerPart == null)
        {
            foreach (var part in allParts) part.PartDisconnected();
            return;
        }

        HashSet<EditorShipPart> visited = new HashSet<EditorShipPart>();

        Queue<EditorShipPart> queue = new Queue<EditorShipPart>();
        queue.Enqueue(centerPart);
        visited.Add(centerPart);

        while (queue.Count > 0)
        {
            EditorShipPart part = queue.Dequeue();

            part.PartConnected();

            if (!adjacency.ContainsKey(part)) continue;

            foreach (var neighbor in adjacency[part])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        foreach (var part in allParts)
        {
            if (!visited.Contains(part))
            {
                part.PartDisconnected();
            }
        }
    }

    #endregion
}
