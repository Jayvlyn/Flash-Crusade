using UnityEngine;

public class PlaceCommand : ICommand
{
    ICommandContext ctx;
    Vector2Int cell;
    Vector2Int cellPlacedAt;

    public PlaceCommand(ICommandContext ctx, Vector2Int cell)
    {
        this.ctx = ctx;
        this.cell = cell;
    }

    public void Execute()
    {
        ShipPart part = ctx.GetHeldPart();
        ctx.PlacePart(part, cell);

        cellPlacedAt = part.cellPlacedAt;

        ctx.ResetScale();

        ctx.SetExpanded(false);
        ctx.NavToCell(part.position);
    }

    public void Undo()
    {
        ctx.SetExpanded(true);
        ShipPart part = ctx.GrabFromGrid(cellPlacedAt);
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

    public bool TryMerge(ICommand next) => false;
}