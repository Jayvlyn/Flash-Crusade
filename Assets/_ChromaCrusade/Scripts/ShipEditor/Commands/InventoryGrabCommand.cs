public class InventoryGrabCommand : ICommand
{
    ICommandContext ctx;
    ShipPartData partData;

    public InventoryGrabCommand(ICommandContext ctx, ShipPartData partData)
    {
        this.ctx = ctx;
        this.partData = partData;
    }

    public void Execute()
    {
        bool success = ctx.TryTakePart(partData, out ShipPart newPart);

        ctx.SetExpanded(true);
        ctx.SwitchToGridMode();

        if (UIManager.Smoothing)
        {
            ctx.GrabFrameLate(newPart, true);
        }
        else
        {
            ctx.UpdateWithRectImmediate(newPart.rect);
            ctx.GrabImmediate(newPart, true);
        }
    }

    public void Undo()
    {
        ctx.AddPart(partData);

        ShipPart heldPart = ctx.GetHeldPart();
        if (heldPart != null) ctx.DestroyPart(heldPart);

        ctx.SetExpanded(false);
        ctx.SwitchToItemMode();
    }

    public void Redo() => Execute();

    public bool TryMerge(ICommand next) => false;
}

