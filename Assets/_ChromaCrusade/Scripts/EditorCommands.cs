using UnityEngine;

public class GrabCommand : IEditorCommand
{
    NavManager nav;
    EditorShipPart part;
    Vector2Int originCell;
    Vector2Int grabbedFromCell;

    public GrabCommand(NavManager nav, EditorShipPart part)
    {
        this.nav = nav;
        this.part = part;
        originCell = part.position;
        grabbedFromCell = nav.currentGridCell;
    }

    public void Execute()
    {
        part = nav.buildArea.GrabPart(nav.currentGridCell);

        nav.visualizer.MatchRectScale(part.rect);

        if (UIManager.Smoothing)
            nav.GrabWithLerp(part, false);
        else
        {
            nav.visualizer.UpdateWithRectImmediate(part.rect);
            nav.GrabImmediate(part, false);
        }
    }

    public void Undo()
    {
        if (part)
        {
            nav.buildArea.PlacePart(originCell, part);
            part.OnPlaced(originCell, nav.buildArea);
        }
        else
        {
            nav.buildArea.PlacePart(originCell, nav.heldPart);
            nav.heldPart.OnPlaced(originCell, nav.buildArea);
        }
        nav.heldPart = null;
        nav.visualizer.ResetScale();

        nav.NavToCell(grabbedFromCell);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class PlaceCommand : IEditorCommand
{
    NavManager nav;
    Vector2Int cell;
    EditorShipPart part;
    Vector2Int cellPlacedAt;

    public PlaceCommand(NavManager nav, Vector2Int cell)
    {
        this.nav = nav;
        this.cell = cell;
    }

    public void Execute()
    {
        part = nav.heldPart;
        nav.buildArea.PlacePart(cell, part);
        part.OnPlaced(cell, nav.buildArea);
        cellPlacedAt = part.cellPlacedAt;
        nav.heldPart = null;
        nav.visualizer.ResetScale();
        nav.NavToCell(part.position);
    }

    public void Undo()
    {
        if (part == null)
        {
            part = nav.buildArea.GrabPart(cellPlacedAt);
        }

        if (part)
        {
            nav.buildArea.GrabPart(part.cellPlacedAt);

            nav.visualizer.MatchRectScale(part.rect);

            if (UIManager.Smoothing)
                nav.GrabWithLerp(part, false);
            else
            {
                nav.visualizer.UpdateWithRectImmediate(part.rect);
                nav.GrabImmediate(part, false);
            }
        }
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class NavigateCommand : IEditorCommand
{
    Vector2 totalInput;
    NavManager nav;

    public NavigateCommand(NavManager nav, Vector2 input)
    {
        this.nav = nav;
        this.totalInput = input;
    }

    public void Execute()
    {
        nav.TriggerNav(totalInput);
    }

    public void Undo()
    {
        nav.TriggerNav(-totalInput);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next)
    {
        if (next is not NavigateCommand other)
            return false;

        totalInput += other.totalInput;

        nav.TriggerNav(other.totalInput);

        return true;
    }
}

public class RotateCommand : IEditorCommand
{
    float angle;
    NavManager nav;

    public RotateCommand(NavManager nav, float angle)
    {
        this.nav = nav;
        this.angle = angle;
    }

    public void Execute()
    {
        nav.RotatePart(angle);
    }

    public void Undo()
    {
        nav.RotatePart(-angle);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class FlipCommand : IEditorCommand
{
    FlipAxis axis;
    NavManager nav;

    public FlipCommand(NavManager nav, FlipAxis axis)
    {
        this.nav = nav;
    }

    public void Execute()
    {
        nav.FlipPart(axis);
    }

    public void Undo()
    {
        nav.FlipPart(axis);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class ExitGridModeCommand : IEditorCommand
{
    private NavManager nav;

    private ShipPartData partData;
    bool xFlipped;
    bool yFlipped;
    float rotation;

    public ExitGridModeCommand(NavManager nav)
    {
        this.nav = nav;

        if (nav.heldPart != null)
        {
            partData = nav.heldPart.partData;
            xFlipped = nav.heldPart.xFlipped;
            yFlipped = nav.heldPart.yFlipped;
            rotation = nav.heldPart.Rotation;
        }
    }

    public void Execute()
    {
        if (partData != null)
            nav.partOrganizer.AddPart(partData);

        if (nav.heldPart != null)
        {
            GameObject.Destroy(nav.heldPart.gameObject);
            nav.heldPart = null;
        }

        nav.SwitchToItemMode();
    }

    public void Undo()
    {
        if (partData != null)
        {
            bool success = nav.partOrganizer.TryTakePart(partData, out EditorShipPart part);

            if (success)
            {
                nav.partOrganizer.SetPartToDefaultStart(part);
                nav.visualizer.UpdateWithRectImmediate(part.rect);
                nav.GrabImmediate(part, true);
            }
        }

        nav.SwitchToGridMode();

        if (nav.heldPart != null) nav.RestorePartTransformations(rotation, xFlipped, yFlipped);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class EnterGridModeCommand : IEditorCommand
{
    private NavManager nav;

    public EnterGridModeCommand(NavManager nav)
    {
        this.nav = nav;
    }

    public void Execute() => nav.SwitchToGridMode();

    public void Undo() => nav.SwitchToItemMode();

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class InventoryGrabCommand : IEditorCommand
{
    private ShipPartData partData;
    private NavManager nav;

    public InventoryGrabCommand(NavManager nav, ShipPartData data)
    {
        this.nav = nav;
        this.partData = data;
    }

    public void Execute()
    {
        bool success = nav.partOrganizer.TryTakePart(partData, out EditorShipPart newPart);

        if (UIManager.Smoothing)
            nav.GrabFrameLate(newPart, true);
        else
        {
            nav.visualizer.UpdateWithRectImmediate(newPart.rect);
            nav.GrabImmediate(newPart, true);
        }

        nav.SwitchToGridMode();
    }

    public void Undo()
    {
        nav.partOrganizer.AddPart(partData);

        if (nav.heldPart != null)
        {
            GameObject.Destroy(nav.heldPart.gameObject);
            nav.heldPart = null;
        }

        nav.SwitchToItemMode();
    }

    public void Redo()
    {
        bool success = nav.partOrganizer.TryTakePart(partData, out EditorShipPart newPart);

        if (!success)
        {
            Debug.LogWarning("Redo failed: part not available.");
            return;
        }

        nav.partOrganizer.SetPartToDefaultStart(newPart);

        if (UIManager.Smoothing)
        {
            nav.GrabWithLerp(newPart, true);
        }
        else
        {
            nav.visualizer.UpdateWithRectImmediate(newPart.rect);
            nav.GrabImmediate(newPart, true);
            nav.SwitchToGridMode();
        }

    }

    public bool TryMerge(IEditorCommand next) => false;
}

public class DeleteCommand : IEditorCommand
{
    NavManager nav;
    Vector2Int startCell;
    Vector2Int partPosition;
    ShipPartData partData;
    bool wasPlaced;
    bool xFlipped;
    bool yFlipped;
    float rotation;

    public DeleteCommand(NavManager nav, Vector2Int startCell)
    {
        this.nav = nav;
        this.startCell = startCell;
    }

    public void Execute()
    {
        if (nav.heldPart != null)
        {
            wasPlaced = false;
            nav.partOrganizer.AddPart(nav.heldPart.partData);
            partData = nav.heldPart.partData;

            xFlipped = nav.heldPart.xFlipped;
            yFlipped = nav.heldPart.yFlipped;
            rotation = nav.heldPart.Rotation;

            GameObject.Destroy(nav.heldPart.gameObject);
        }
        else
        {
            wasPlaced = true;
            EditorShipPart part = nav.buildArea.GrabPart(nav.currentGridCell);
            partData = part.partData;
            partPosition = part.position;
            nav.partOrganizer.AddPart(part.partData);
            nav.GrabImmediate(part, false);

            xFlipped = nav.heldPart.xFlipped;
            yFlipped = nav.heldPart.yFlipped;
            rotation = nav.heldPart.Rotation;

            GameObject.Destroy(part.gameObject);
        }

        nav.heldPart = null;
        nav.NavToCell(startCell);
    }

    public void Undo()
    {
        if (partData != null)
        {
            nav.midUndoDelete = true;
            if (wasPlaced) nav.visualizer.HighlightCellImmediate(partPosition, true);
            else nav.visualizer.HighlightCellImmediate(startCell, true);

            nav.StartCoroutine(nav.UndoDeleteRoutine(wasPlaced, partData, partPosition, startCell, rotation, xFlipped, yFlipped));
        }
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;

}

public class ResetCommand : IEditorCommand
{
    NavManager nav;
    Vector2Int prevCell;

    public ResetCommand(NavManager nav)
    {
        this.nav = nav;
        prevCell = nav.currentGridCell;
    }

    public void Execute()
    {
        nav.InitNavMode(true);
    }

    public void Undo()
    {
        nav.NavToCell(prevCell);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;

}

