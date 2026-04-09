using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Part")]
public class ShipPartData : ScriptableObject
{
    public Sprite sprite;
    public int mass;
    public int price => mass * 10;
    public virtual PartType PartType { get; }

    public PartSegment[] segments;
}