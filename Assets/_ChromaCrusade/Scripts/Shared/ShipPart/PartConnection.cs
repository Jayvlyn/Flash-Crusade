[System.Serializable]
public class PartConnection
{
    public PartConnection() => connectionState = ConnectionState.Disabled;
    public PartConnection(ConnectionState startState) => connectionState = startState;

    public ConnectionState connectionState;
}