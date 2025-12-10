using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Part")]
public class ShipPartData : ScriptableObject
{
    public Sprite sprite;
    public float mass;
    public int price;

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

[System.Serializable]
public class PartSegment
{
    public SegmentState segmentState;
    public PartConnection topConnection;
    public PartConnection leftConnection;
    public PartConnection rightConnection;
    public PartConnection bottomConnection;
}

[System.Serializable]
public class PartConnection
{
    public ConnectionState connectionState;
}

public enum ConnectionState { Blocked, Disabled, Enabled }
public enum SegmentState { Disabled, Enabled }
public enum PartType
{
    Cabin = 1,
    Core = 2,
    Wing = 3,
    Weapon = 4,
    Utility = 5
}