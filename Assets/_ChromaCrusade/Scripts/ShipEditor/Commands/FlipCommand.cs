public class FlipCommand : ICommand
{
    ICommandContext ctx;
    FlipAxis axis;

    public FlipCommand(ICommandContext ctx, FlipAxis axis)
    {
        this.ctx = ctx;

        this.axis = axis;
    }

    public void Execute() => ctx.FlipPart(axis);

    public void Undo() => ctx.FlipPart(axis);

    public void Redo() => Execute();

    public bool TryMerge(ICommand next) => false;
}