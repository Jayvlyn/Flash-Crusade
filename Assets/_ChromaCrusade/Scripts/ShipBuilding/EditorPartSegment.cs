using UnityEngine;

public class EditorPartSegment
{
    public EditorPartSegment() => Init(SegmentState.Disabled);

    public EditorPartSegment(SegmentState startState) => Init(startState);
    
    private void Init(SegmentState state)
    {
        segmentState = state;
        topConnection = new EditorPartConnection();
        leftConnection = new EditorPartConnection();
        rightConnection = new EditorPartConnection();
        bottomConnection = new EditorPartConnection();
    }

    public SegmentState segmentState;
    public EditorPartConnection topConnection;
    public EditorPartConnection leftConnection;
    public EditorPartConnection rightConnection;
    public EditorPartConnection bottomConnection;
}

public class EditorPartConnection
{
    public EditorPartConnection() => connectionState = ConnectionState.Disabled;
    public EditorPartConnection(ConnectionState startState) => connectionState = startState;

    public ConnectionState connectionState;
}
