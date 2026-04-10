
using UnityEngine;

public struct UIShipData
{
    public Sprite shipSprite;
    public string shipName;

    public UIShipData(Sprite shipSprite, string shipName)
    {
        this.shipSprite = shipSprite;
        this.shipName = shipName;
    }
}