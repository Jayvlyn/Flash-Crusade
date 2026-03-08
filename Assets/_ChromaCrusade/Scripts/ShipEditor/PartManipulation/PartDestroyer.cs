using System.Collections;
using UnityEngine;

public class PartDestroyer : MonoBehaviour, IPartDestroyer
{
    public IPartGrabber grabber;
    public IPartTransformer transformer;
    public IPartPlacer placer;
    public IEditorNavVisualizer visualizer;
    public IGridNavigator gridNav;
    public IInventoryManager inventory;

    #region IPartDestroyer

    public void DestroyPart(EditorShipPart part)
    {
        if (part == EditorState.heldPart) EditorState.heldPart = null;
        Destroy(part.gameObject);
    }

    public void HandleUndoRoutine(bool wasPlaced, ShipPartData partData, Vector2Int partPosition, Vector2Int startCell, float rotation, bool xFlipped = false, bool yFlipped = false)
    {
        StartCoroutine(UndoDeleteRoutine(wasPlaced, partData, partPosition, startCell, rotation, xFlipped, yFlipped));
    }

    #endregion

    IEnumerator UndoDeleteRoutine(bool wasPlaced, ShipPartData partData, Vector2Int partPosition, Vector2Int startCell, float rotation, bool xFlipped = false, bool yFlipped = false)
    {
        EditorState.midUndoDelete = true;
        bool success = inventory.TryTakePart(partData, out EditorShipPart part);
        if (success) grabber.GrabImmediate(part, true);
        yield return null;
        transformer.RestoreHeldPartTransformations(rotation, xFlipped, yFlipped);
        yield return null;

        if (wasPlaced)
        {
            placer.PlacePart(part, partPosition);

            visualizer.ResetScale();
            visualizer.SetExpanded(false);
            gridNav.NavToCell(startCell);
        }
        else
        {
            visualizer.SetExpanded(true);
        }
        EditorState.midUndoDelete = false;
    }
}
