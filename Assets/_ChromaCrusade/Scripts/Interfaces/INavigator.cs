using UnityEngine;

public interface INavigator
{
    void TriggerNav(Vector2 dir);
    void NavToCell(Vector2Int cell);
    void InitNavMode();
    void ResetGridPosition();
    Vector2Int GetCurrentGridCell();

    void SwitchToGridMode();
    void SwitchToItemMode();
}
