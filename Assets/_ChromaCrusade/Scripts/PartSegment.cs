using UnityEngine;

[System.Serializable]
public class PartSegment
{
    public PartSegment() => Init(SegmentState.Disabled);

    public PartSegment(SegmentState startState) => Init(startState);
    
    private void Init(SegmentState state)
    {
        segmentState = state;
        topConnection = new PartConnection();
        leftConnection = new PartConnection();
        rightConnection = new PartConnection();
        bottomConnection = new PartConnection();
    }

    public SegmentState segmentState;
    public PartConnection topConnection;
    public PartConnection leftConnection;
    public PartConnection rightConnection;
    public PartConnection bottomConnection;
}
