using UnityEngine;

public interface IEditorCommandContext :
    IInventoryManager,
    INavigator,
    IPartDestroyer,
    IPartPlacer,
    IPartGrabber,
    IPartTransformer,
    IVisualizer
{ }