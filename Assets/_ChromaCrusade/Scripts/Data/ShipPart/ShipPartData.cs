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

    public virtual void Apply(ImporterPart importer)
    {
        sprite = importer.partSprite;
        mass = importer.mass;
        price = importer.price;

        segments = new PartSegment[importer.segments.Length];

        for (int i = 0; i < importer.segments.Length; i++)
        {
            var source = importer.segments[i];
            var seg = new PartSegment
            {
                segmentState = source.segmentState,
                topConnection = new PartConnection { connectionState = source.topConnection.connectionState },
                leftConnection = new PartConnection { connectionState = source.leftConnection.connectionState },
                rightConnection = new PartConnection { connectionState = source.rightConnection.connectionState },
                bottomConnection = new PartConnection { connectionState = source.bottomConnection.connectionState }
            };

            segments[i] = seg;
        }
    }
}