using UnityEngine;

public interface IEditorCommand
{
    void Execute();
    void Undo();
    bool TryMerge(IEditorCommand next);
}
