using UnityEngine;

public class GrabCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    Vector2Int partCenterCell;
    Vector2Int grabbedFromCell;

    public GrabCommand(IEditorCommandContext ctx, Vector2Int partCenterCell, Vector2Int grabbingFromCell)
    {
        this.ctx = ctx;
        this.partCenterCell = partCenterCell;
        grabbedFromCell = grabbingFromCell;
    }

    public void Execute()
    {
        ctx.SetExpanded(true);
        EditorShipPart part = ctx.GrabFromGrid(grabbedFromCell);
        ctx.MatchRectScale(part.rect);

        if (UIManager.Smoothing)
            ctx.GrabWithLerp(part, false);
        else
        {
            ctx.UpdateWithRectImmediate(part.rect);
            ctx.GrabImmediate(part, false);
        }
    }

    public void Undo()
    {
        EditorShipPart part = ctx.GetHeldPart();
        ctx.PlacePart(part, partCenterCell);
        ctx.ResetScale();
        ctx.SetExpanded(false);
        ctx.NavToCell(grabbedFromCell);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class PlaceCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    Vector2Int cell;
    Vector2Int cellPlacedAt;

    public PlaceCommand(IEditorCommandContext ctx, Vector2Int cell)
    {
        this.ctx = ctx;
        this.cell = cell;
    }

    public void Execute()
    {
        EditorShipPart part = ctx.GetHeldPart();
        ctx.PlacePart(part, cell);

        cellPlacedAt = part.cellPlacedAt;

        ctx.ResetScale();

        ctx.SetExpanded(false);
        ctx.NavToCell(part.position);
    }

    public void Undo()
    {
        ctx.SetExpanded(true);
        EditorShipPart part = ctx.GrabFromGrid(cellPlacedAt);
        ctx.MatchRectScale(part.rect);

        if (UIManager.Smoothing)
            ctx.GrabWithLerp(part, false);
        else
        {
            ctx.UpdateWithRectImmediate(part.rect);
            ctx.GrabImmediate(part, false);
        }
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class NavigateCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    Vector2 totalInput;

    public NavigateCommand(IEditorCommandContext ctx, Vector2 input)
    {
        this.ctx = ctx;
        this.totalInput = input;
    }

    public void Execute() => ctx.TriggerNav(totalInput);

    public void Undo() => ctx.TriggerNav(-totalInput);

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next)
    {
        if (next is not NavigateCommand other)
            return false;

        totalInput += other.totalInput;

        ctx.TriggerNav(other.totalInput);

        return true;
    }
}

public class RotateCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    float angle;

    public RotateCommand(IEditorCommandContext ctx, float angle)
    {
        this.ctx = ctx;
        this.angle = angle;
    }

    public void Execute() => ctx.RotatePart(angle);

    public void Undo() => ctx.RotatePart(-angle);

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class FlipCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    FlipAxis axis;

    public FlipCommand(IEditorCommandContext ctx, FlipAxis axis)
    {
        this.ctx = ctx;

        this.axis = axis;
    }

    public void Execute() => ctx.FlipPart(axis);

    public void Undo() => ctx.FlipPart(axis);

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class ExitGridModeCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    ShipPartData partData;
    bool xFlipped;
    bool yFlipped;
    float rotation;

    public ExitGridModeCommand(IEditorCommandContext ctx, EditorShipPart heldPart)
    {
        this.ctx = ctx;

        if (heldPart != null)
        {
            partData = heldPart.partData;
            xFlipped = heldPart.xFlipped;
            yFlipped = heldPart.yFlipped;
            rotation = heldPart.Rotation;
        }
    }

    public void Execute()
    {
        if (partData != null)
        {
            ctx.AddPart(partData);
            EditorShipPart heldPart = ctx.GetHeldPart();
            ctx.DestroyPart(heldPart);
        }

        ctx.SetExpanded(false);
        ctx.SwitchToItemMode();
    }

    public void Undo()
    {
        if (partData != null)
        {
            bool success = ctx.TryTakePart(partData, out EditorShipPart part);

            if (success)
            {
                ctx.SetExpanded(true);
                ctx.SetPartToDefaultStart(part);
                ctx.UpdateWithRectImmediate(part.rect);
                ctx.GrabImmediate(part, true);
            }
        }

        ctx.SwitchToGridMode();

        if (ctx.GetHeldPart() != null)
            ctx.RestorePartTransformations(rotation, xFlipped, yFlipped);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class EnterGridModeCommand : IEditorCommand
{
    IEditorCommandContext ctx;

    public EnterGridModeCommand(IEditorCommandContext ctx)
    {
        this.ctx = ctx;
    }

    public void Execute() => ctx.SwitchToGridMode();

    public void Undo() => ctx.SwitchToItemMode();

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;
}

public class InventoryGrabCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    ShipPartData partData;

    public InventoryGrabCommand(IEditorCommandContext ctx, ShipPartData partData)
    {
        this.ctx = ctx;
        this.partData = partData;
    }

    public void Execute()
    {
        bool success = ctx.TryTakePart(partData, out EditorShipPart newPart);

        if (UIManager.Smoothing)
            ctx.GrabFrameLate(newPart, true);
        else
        {
            ctx.UpdateWithRectImmediate(newPart.rect);
            ctx.GrabImmediate(newPart, true);
        }

        ctx.SetExpanded(true);
        ctx.SwitchToGridMode();
    }

    public void Undo()
    {
        ctx.AddPart(partData);

        EditorShipPart heldPart = ctx.GetHeldPart();
        if (heldPart != null) ctx.DestroyPart(heldPart);

        ctx.SetExpanded(false);
        ctx.SwitchToItemMode();
    }

    public void Redo()
    {
        bool success = ctx.TryTakePart(partData, out EditorShipPart newPart);

        if (!success) return;

        ctx.SetPartToDefaultStart(newPart);

        if (UIManager.Smoothing)
        {
            ctx.SetExpanded(true);
            ctx.GrabWithLerp(newPart, true);
        }
        else
        {
            ctx.UpdateWithRectImmediate(newPart.rect);
            ctx.GrabImmediate(newPart, true);
            ctx.SetExpanded(true);
            ctx.SwitchToGridMode();
        }
    }

    public bool TryMerge(IEditorCommand next) => false;
}

public class DeleteCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    Vector2Int startCell;
    Vector2Int partPosition;
    ShipPartData partData;
    bool wasPlaced;
    bool xFlipped;
    bool yFlipped;
    float rotation;

    public DeleteCommand(IEditorCommandContext ctx, Vector2Int startCell)
    {
        this.ctx = ctx;
        this.startCell = startCell;
    }

    public void Execute()
    {
        EditorShipPart heldPart = ctx.GetHeldPart();

        if (heldPart != null)
        {
            wasPlaced = false;

            ctx.AddPart(heldPart.partData);

            partData = heldPart.partData;
            xFlipped = heldPart.xFlipped;
            yFlipped = heldPart.yFlipped;
            rotation = heldPart.Rotation;

            ctx.DestroyPart(heldPart);
        }
        else
        {
            wasPlaced = true;

            EditorShipPart part = ctx.GrabFromGrid(ctx.GetCurrentGridCell());
            partData = part.partData;
            partPosition = part.position;
            ctx.AddPart(part.partData);
            ctx.GrabImmediate(part, false);

            xFlipped = part.xFlipped;
            yFlipped = part.yFlipped;
            rotation = part.Rotation;

            ctx.DestroyPart(part);
        }

        ctx.ResetScale();
        ctx.SetExpanded(false);
        ctx.NavToCell(startCell);
    }

    public void Undo()
    {
        if (partData != null)
        {
            ctx.SetExpanded(true);

            if (wasPlaced) ctx.HighlightCellImmediate(partPosition);
            else ctx.HighlightCellImmediate(startCell);

            ctx.HandleUndoRoutine(wasPlaced, partData, partPosition, startCell, rotation, xFlipped, yFlipped);
        }
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;

}

public class ResetCommand : IEditorCommand
{
    IEditorCommandContext ctx;
    Vector2Int prevCell;

    public ResetCommand(IEditorCommandContext ctx)
    {
        this.ctx = ctx;
        prevCell = ctx.GetCurrentGridCell();
    }

    public void Execute()
    {
        ctx.ResetGridPosition();
        ctx.InitNavMode();
    }

    public void Undo()
    {
        ctx.NavToCell(prevCell);
    }

    public void Redo() => Execute();

    public bool TryMerge(IEditorCommand next) => false;

}

