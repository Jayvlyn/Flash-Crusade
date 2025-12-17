using UnityEngine;

public class ResetCommand : ICommand
{
    ICommandContext ctx;
    Vector2Int prevCell;

    public ResetCommand(ICommandContext ctx)
    {
        this.ctx = ctx;
        prevCell = ctx.GetCurrentGridCell();
    }

    public void Execute()
    {
        ctx.ResetGridPosition();
        ctx.InitGridMode();
    }

    public void Undo()
    {
        ctx.NavToCell(prevCell);
    }

    public void Redo() => Execute();

    public bool TryMerge(ICommand next) => false;

}
