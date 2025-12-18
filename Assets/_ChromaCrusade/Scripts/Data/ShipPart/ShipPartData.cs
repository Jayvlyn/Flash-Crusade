using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Part")]
public class ShipPartData : ScriptableObject
{
    public Sprite sprite;
    public float mass;
    public int price;
    public virtual PartType PartType { get; }

    public PartSegment[] segments;
}