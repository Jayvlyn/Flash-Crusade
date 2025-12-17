using System.Collections.Generic;
using UnityEngine;

public class CommandHistory : MonoBehaviour
{
    private static Stack<ICommand> undoStack = new Stack<ICommand>();
    private static Stack<ICommand> redoStack = new Stack<ICommand>();

    public static void Execute(ICommand command)
    {
        if (undoStack.Count > 0)
        {
            var last = undoStack.Peek();

            if(last.TryMerge(command))
            {
                return;
            }
        }

        command.Execute();
        undoStack.Push(command);
        redoStack.Clear();
    }

    public static void Undo()
    {
        if (undoStack.Count == 0) return;

        ICommand command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
    }

    public static void Redo()
    {
        if (redoStack.Count == 0) return;
        ICommand command = redoStack.Pop();
        command.Redo();
        undoStack.Push(command);
    }
}
