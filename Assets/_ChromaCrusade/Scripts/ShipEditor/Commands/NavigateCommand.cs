using UnityEngine;

public class NavigateCommand : ICommand
{
    ICommandContext ctx;
    Vector2 totalInput;

    public NavigateCommand(ICommandContext ctx, Vector2 input, EditorState state)
    {
        this.ctx = ctx;
        this.totalInput = input;
    }

    public void Execute() => ctx.TriggerGridNav(totalInput);

    public void Undo() => ctx.TriggerGridNav(-totalInput);

    public void Redo() => Execute();

    public bool TryMerge(ICommand next)
    {
        if (next is not NavigateCommand other)
            return false;

        totalInput += other.totalInput;

        ctx.TriggerGridNav(other.totalInput);

        return true;
    }
}