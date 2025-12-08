using UnityEngine;

public class EditorShipPart : MonoBehaviour
{
    public enum PartState { Inventory, Grabbed, PlacedConnected, PlacedDisconnected}
    private PartState partState;

    public EditorPartSegment[,] segments = new EditorPartSegment[3,3];
    /*
        (0,0) (1,0) (2,0)
        (0,1) (1,1) (2,1)
        (0,2) (1,2) (2,2)
    */

    public Vector2Int position; // center segment
    public Vector2Int lastGrabbedFromCell;

    public bool xFlipped;
    public bool yFlipped;

    private float rotation;
    public float Rotation { get { return rotation; } }

    public RectTransform rect;
    public RectTransformFollower rtf;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        rtf = GetComponent<RectTransformFollower>();
        rtf.enabled = false;

        //TESTING:
        segments[0,0] = new EditorPartSegment();
        segments[2,0] = new EditorPartSegment();
        segments[0,1] = new EditorPartSegment();
        segments[1,1] = new EditorPartSegment();
        segments[2,1] = new EditorPartSegment();
        segments[0,2] = new EditorPartSegment();
        segments[1,2] = new EditorPartSegment();
    }

    #region State

    private void ChangeState(PartState newState)
    {
        OnExitState();
        partState = newState;
        OnEnterState();

    }
    private void OnExitState()
    {
        switch (partState)
        {
            case PartState.Grabbed:
                rtf.enabled = false;
                rtf.target = null;
                break;
            default:
                break;
        }
    }
    private void OnEnterState()
    {
        switch (partState)
        {
            case PartState.Grabbed:
                rtf.enabled = true;
                break;
            default:
                break;
        }
    }

    #endregion

    #region Manipulation

    public void OnGrabbed(RectTransform visualizerRect)
    {
        ChangeState(PartState.Grabbed);
        rect.parent = visualizerRect.parent;
        int currentIndex = rect.GetSiblingIndex();
        rect.SetSiblingIndex(Mathf.Max(0, currentIndex - 1));
        rtf.target = visualizerRect;
    }

    public void OnPlaced(Vector2Int position, EditorBuildArea buildArea)
    {
        this.position = position;
        rect.parent = buildArea.rect;
        ChangeState(PartState.PlacedDisconnected);
    }

    public void Rotate(float angle)
    {
        rotation -= angle;
        if (rotation > 270) rotation = 0;
        if (rotation < 0) rotation = 270;
    }

    public void Flip(bool horizontal)
    {
        if (horizontal)
            xFlipped = !xFlipped;
        else
            yFlipped = !yFlipped;
    }

    #endregion
}
