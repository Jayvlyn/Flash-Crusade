using UnityEngine;

public interface IUINavigator
{
    void Init();
    void TriggerItemNav(Vector2 dir);
    void NavToItem(NavItem item);
    void SwitchOff();
}
