using System.Collections;
using UnityEngine;

public class PartGrabber : MonoBehaviour, IPartGrabber
{
    [HideInInspector] public BuildArea buildArea;
    public EditorState EditorState { get; set; }

    public IUINavigator uiNav;
    public IVisualizer visualizer;

    #region IPartGrabber

    public void GrabFrameLate(ShipPart part, bool fromInv)
    {
        EditorState.heldPart = part;
        StartCoroutine(GrabFrameLateRoutine(part, fromInv));
    }

    public ShipPart GrabFromGrid(Vector2Int cell)
    {
        return buildArea.GrabPart(cell);
    }

    public void GrabImmediate(ShipPart part, bool fromInv)
    {
        part.OnGrabbed(visualizer.GetRect());
        if (!fromInv) EditorState.currentGridCell = part.position;
        EditorState.heldPart = part;
    }

    public void GrabWithLerp(ShipPart part, bool fromInv)
    {
        StartCoroutine(GrabWithLerpRoutine(part, fromInv));
    }

    #endregion

    IEnumerator GrabFrameLateRoutine(ShipPart part, bool fromInv)
    {
        yield return null;
        visualizer.UpdateWithRectImmediate(part.rect);
        GrabImmediate(part, fromInv);
    }

    IEnumerator GrabWithLerpRoutine(ShipPart part, bool fromInv)
    {
        EditorState.midGrab = true;
        yield return visualizer.LerpWithRect(part.rect); // waits until done

        part.OnGrabbed(visualizer.GetRect());
        if (!fromInv) EditorState.currentGridCell = part.position;
        EditorState.heldPart = part;
        EditorState.midGrab = false;
        if (fromInv) uiNav.SwitchToGridMode();
    }
}
