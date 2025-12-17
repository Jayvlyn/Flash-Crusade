public class ExitGridModeCommand : ICommand
{
    ICommandContext ctx;
    ShipPartData partData;
    bool xFlipped;
    bool yFlipped;
    float rotation;

    public ExitGridModeCommand(ICommandContext ctx, ShipPart heldPart)
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
            ShipPart heldPart = ctx.GetHeldPart();
            ctx.DestroyPart(heldPart);
        }

        ctx.SetExpanded(false);
        ctx.SwitchToItemMode();
    }

    public void Undo()
    {
        if (partData != null)
        {
            bool success = ctx.TryTakePart(partData, out ShipPart part);

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

    public bool TryMerge(ICommand next) => false;
}
