using System.Collections;
using UnityEngine;

public class PartGrabber : MonoBehaviour, IPartGrabber
{
    [HideInInspector] public BuildArea buildArea;

    public IUINavigator uiNav;
    public IEditorNavVisualizer visualizer;

    #region IPartGrabber

    public void GrabFrameLate(EditorShipPart part, bool fromInv)
    {
        EditorState.heldPart = part;
        StartCoroutine(GrabFrameLateRoutine(part, fromInv));
    }

    public EditorShipPart GrabFromGrid(Vector2Int cell)
    {
        return buildArea.GrabPart(cell);
    }

    public void GrabImmediate(EditorShipPart part, bool fromInv)
    {
        part.OnGrabbed(visualizer.GetRect());
        if (!fromInv) EditorState.CurrentGridCell = part.position;
        EditorState.heldPart = part;
    }

    public void GrabWithLerp(EditorShipPart part, bool fromInv)
    {
        StartCoroutine(GrabWithLerpRoutine(part, fromInv));
    }

    #endregion

    IEnumerator GrabFrameLateRoutine(EditorShipPart part, bool fromInv)
    {
        yield return null;
        visualizer.UpdateWithRectImmediate(part.rect);
        GrabImmediate(part, fromInv);
    }

    IEnumerator GrabWithLerpRoutine(EditorShipPart part, bool fromInv)
    {
        EditorState.midGrab = true;
        yield return visualizer.LerpWithRect(part.rect); // waits until done

        part.OnGrabbed(visualizer.GetRect());
        if (!fromInv) EditorState.CurrentGridCell = part.position;
        EditorState.heldPart = part;
        EditorState.midGrab = false;
        if (fromInv) uiNav.SwitchOff();
    }
}