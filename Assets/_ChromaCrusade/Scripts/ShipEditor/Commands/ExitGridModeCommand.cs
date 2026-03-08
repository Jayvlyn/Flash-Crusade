public class ExitGridModeCommand : ICommand
{
    ICommandContext ctx;
    ShipPartData partData;
    bool xFlipped;
    bool yFlipped;
    float rotation;

    public ExitGridModeCommand(ICommandContext ctx, EditorShipPart heldPart)
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
        ctx.SwitchOff();

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

        if (ctx.GetHeldPart() != null)
            ctx.RestoreHeldPartTransformations(rotation, xFlipped, yFlipped);
    }

    public void Redo() => Execute();

    public bool TryMerge(ICommand next) => false;
}
