using System.Collections.Generic;
using UnityEngine;

public class CommandHistory : MonoBehaviour
{
    private static Stack<IEditorCommand> undoStack = new Stack<IEditorCommand>();
    private static Stack<IEditorCommand> redoStack = new Stack<IEditorCommand>();

    public static void Execute(IEditorCommand command)
    {
        command.Execute();
        undoStack.Push(command);
        redoStack.Clear();
    }

    public static void Undo()
    {
        Debug.Log("UNDO");
        if (undoStack.Count == 0) return;

        IEditorCommand command = undoStack.Pop();
        command.Undo();
        redoStack.Push(command);
    }

    public static void Redo()
    {
        Debug.Log("REDO");
        if (redoStack.Count == 0) return;
        IEditorCommand command = redoStack.Pop();
        command.Execute();
        undoStack.Push(command);
    }
}
