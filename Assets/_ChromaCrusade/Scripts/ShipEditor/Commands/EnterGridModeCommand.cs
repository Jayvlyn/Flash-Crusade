public class EnterGridModeCommand : ICommand
{
    ICommandContext ctx;

    public EnterGridModeCommand(ICommandContext ctx)
    {
        this.ctx = ctx;
    }

    public void Execute() => ctx.SwitchToGridMode();

    public void Undo() => ctx.SwitchToItemMode();

    public void Redo() => Execute();

    public bool TryMerge(ICommand next) => false;
}