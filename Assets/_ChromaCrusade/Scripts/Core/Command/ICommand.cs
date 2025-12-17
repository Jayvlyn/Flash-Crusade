using UnityEngine;

public interface ICommand
{
    void Execute();
    void Undo();
    void Redo();
    bool TryMerge(ICommand next);
}
