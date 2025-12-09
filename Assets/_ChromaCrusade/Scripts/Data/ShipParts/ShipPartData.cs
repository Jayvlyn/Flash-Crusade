using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Part")]
public class ShipPartData : ScriptableObject
{
    public Sprite sprite;
    public float mass;
    public int price;

    public virtual void Apply(ImporterPart importer)
    {
        sprite = importer.partSprite;
        mass = importer.mass;
        price = importer.price;
    }
}