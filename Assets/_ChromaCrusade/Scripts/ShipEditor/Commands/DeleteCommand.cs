using UnityEngine;

public class DeleteCommand : ICommand
{
    ICommandContext ctx;
    Vector2Int startCell;
    Vector2Int partPosition;
    ShipPartData partData;
    bool wasPlaced;
    bool xFlipped;
    bool yFlipped;
    float rotation;

    public DeleteCommand(ICommandContext ctx, Vector2Int startCell)
    {
        this.ctx = ctx;
        this.startCell = startCell;
    }

    public void Execute()
    {
        ShipPart heldPart = ctx.GetHeldPart();

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

            ShipPart part = ctx.GrabFromGrid(ctx.GetCurrentGridCell());
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

    public bool TryMerge(ICommand next) => false;

}
