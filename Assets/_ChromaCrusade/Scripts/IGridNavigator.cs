using UnityEngine;

public interface IGridNavigator
{
    void TriggerGridNav(Vector2 dir);
    void NavToCell(Vector2Int cell);
    Vector2Int GetCurrentGridCell();
    void ResetGridPosition();
    void InitGridMode();
    void SwitchToItemMode();
}
