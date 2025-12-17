using UnityEngine;

public interface IUINavigator
{
    void InitItemMode();
    void TriggerItemNav(Vector2 dir);
    void SwitchToGridMode();
}
