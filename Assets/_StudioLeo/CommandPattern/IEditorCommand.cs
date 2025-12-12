using UnityEngine;

public interface IEditorCommand
{
    void Execute();
    void Undo();
    void Redo();
    bool TryMerge(IEditorCommand next);
}
