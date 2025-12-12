using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorShipPart : MonoBehaviour
{
    public enum PartState { Inventory, Grabbed, PlacedConnected, PlacedDisconnected}
    public PartState partState;

    public EditorPartSegment[,] segments = new EditorPartSegment[3,3];
    /*
        (0,0) (1,0) (2,0)
        (0,1) (1,1) (2,1)
        (0,2) (1,2) (2,2)
    */

    [HideInInspector] public Vector2Int position; // center segment
    [HideInInspector] public Vector2Int cellPlacedAt;

    [HideInInspector] public bool xFlipped;
    [HideInInspector] public bool yFlipped;

    private float rotation;
    public float Rotation { get { return rotation; } }

    [HideInInspector] public RectTransform rect;
    [HideInInspector] public RectTransformFollower rtf;
    [HideInInspector] public Image image;

    public RectTransform partSelectorRect;

    public ShipPartData partData;

    public bool debugState = false;

    private void Update()
    {
        if(debugState)
        {
            Debug.Log(partState.ToString());
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        rtf = GetComponent<RectTransformFollower>();
        rtf.enabled = false;

        for(int x = 0; x < 3; x++)
        {
            for(int  y = 0; y < 3; y++)
            {
                segments[x, y] = new EditorPartSegment();
            }
        }
    }

    public void Init(ShipPartData partData)
    {
        this.partData = partData;
        image.sprite = partData.sprite;

        for(int i = 0; i < 9; i++)
        {
            int width = 3;
            int x = i % width;
            int y = i / width;

            EditorPartSegment segment = segments[x, y];
            PartSegment dataSegment = partData.segments[i];

            segment.segmentState = dataSegment.segmentState;
            segment.topConnection.connectionState = dataSegment.topConnection.connectionState;
            segment.leftConnection.connectionState = dataSegment.leftConnection.connectionState;
            segment.rightConnection.connectionState = dataSegment.rightConnection.connectionState;
            segment.bottomConnection.connectionState = dataSegment.bottomConnection.connectionState;
        }
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
            case PartState.Inventory:
                break;
            case PartState.PlacedConnected:
                break;
            case PartState.PlacedDisconnected:
                ChangeAlpha(1f);
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
            case PartState.Inventory:
                break;
            case PartState.PlacedConnected:
                break;
            case PartState.PlacedDisconnected:
                ChangeAlpha(0.3f);
                break;
            default:
                break;
        }
    }

    public void PartConnected()
    {
        ChangeState(PartState.PlacedConnected);
    }

    public void PartDisconnected()
    {
        ChangeState(PartState.PlacedDisconnected);
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
    }

    public void Rotate(float angle)
    {
        rotation += angle;
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

    private void ChangeAlpha(float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
