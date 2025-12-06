using UnityEngine;

public class EditorShipPart : MonoBehaviour
{
    public enum PartState { Inventory, Grabbed, PlacedConnected, PlacedDisconnected}
    private PartState partState;

    private EditorPartSegment TopLeftSegment,    TopMiddleSegment,    TopRightSegment;
    private EditorPartSegment MiddleLeftSegment, MiddleSegment,       MiddleRightSegment;
    private EditorPartSegment BottomLeftSegment, BottomMiddleSegment, BottomRightSegment;
    #region Shortcuts
    private EditorPartSegment TLS => TopLeftSegment;
    private EditorPartSegment TMS => TopMiddleSegment;
    private EditorPartSegment TRS => TopRightSegment;
    private EditorPartSegment MLS => MiddleLeftSegment;
    private EditorPartSegment MS  => MiddleSegment;
    private EditorPartSegment MRS => MiddleRightSegment;
    private EditorPartSegment BLS => BottomLeftSegment;
    private EditorPartSegment BMS => BottomMiddleSegment;
    private EditorPartSegment BRS => BottomRightSegment;
    #endregion

    private bool xFlipped;
    private bool yFlipped;

    private float rotation;

    private Vector2Int position; // center segment

    public RectTransformFollower rtf;

    private void Awake()
    {
        rtf = GetComponent<RectTransformFollower>();
        rtf.enabled = false;
    }

    public void Rotate(bool cw)
    {
        rotation = cw ? rotation + 90 : rotation - 90;
        if (rotation > 270) rotation = 0;
        if (rotation < 0) rotation = 270;

        // do actual transform stuff here
        // transform.rotation = rotation
        // or
        // lerp transform.rotation to rotation
    }

    public void ChangeState(PartState newState)
    {
        partState = newState;

        switch(newState)
        {
            case PartState.Grabbed:
                rtf.enabled = true;
                break;
        }
    }
}
