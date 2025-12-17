using UnityEngine;

public class GrabCommand : ICommand
{
    ICommandContext ctx;
    Vector2Int partCenterCell;
    Vector2Int grabbedFromCell;

    public GrabCommand(ICommandContext ctx, Vector2Int partCenterCell, Vector2Int grabbingFromCell)
    {
        this.ctx = ctx;
        this.partCenterCell = partCenterCell;
        grabbedFromCell = grabbingFromCell;
    }

    public void Execute()
    {
        ctx.SetExpanded(true);
        ShipPart part = ctx.GrabFromGrid(grabbedFromCell);
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
        ShipPart part = ctx.GetHeldPart();
        ctx.PlacePart(part, partCenterCell);
        ctx.ResetScale();
        ctx.SetExpanded(false);
        ctx.NavToCell(grabbedFromCell);
    }

    public void Redo() => Execute();

    public bool TryMerge(ICommand next) => false;
}