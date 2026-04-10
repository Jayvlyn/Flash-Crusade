using UnityEngine;
using UnityEngine.UI;

public class UIShip : MonoBehaviour
{
    [SerializeField] RectTransform rect;
    [SerializeField] Image shipImage;

    UIShipData shipData;

    public string ShipName => shipData.shipName;
    public Sprite ShipSprite => shipData.shipSprite;

    public void Init(Sprite sprite, string name)
    {
        shipData.shipSprite = sprite;
        shipData.shipName = name;

        shipImage.sprite = sprite;
    }
}
