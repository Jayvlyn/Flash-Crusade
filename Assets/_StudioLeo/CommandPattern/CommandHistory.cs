using System.Collections.Generic;
using UnityEngine;

public class CommandHistory : MonoBehaviour
{
    private static Stack<IEditorCommand> undoStack = new Stack<IEditorCommand>();
    private static Stack<IEditorCommand> redoStack = new Stack<IEditorCommand>();

    public static void Execute(IEditorCommand command)
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

        IEditorCommand command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
    }

    public static void Redo()
    {
        if (redoStack.Count == 0) return;
        IEditorCommand command = redoStack.Pop();
        command.Redo();
        undoStack.Push(command);
    }
}
