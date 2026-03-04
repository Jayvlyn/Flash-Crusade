using UnityEngine;
using UnityEngine.UI;

public class UIShip : MonoBehaviour
{
    [SerializeField] RectTransform rect;
    [SerializeField] Image shipImage;

    public Sprite shipSprite;
    public string shipName;

    public void Init(Sprite sprite, string name)
    {
        shipSprite = sprite;
        shipName = name;

        shipImage.sprite = shipSprite;
    }
}
