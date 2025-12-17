public class RotateCommand : ICommand
{
    ICommandContext ctx;
    float angle;

    public RotateCommand(ICommandContext ctx, float angle)
    {
        this.ctx = ctx;
        this.angle = angle;
    }

    public void Execute() => ctx.RotatePart(angle);

    public void Undo() => ctx.RotatePart(-angle);

    public void Redo() => Execute();

    public bool TryMerge(ICommand next) => false;
}