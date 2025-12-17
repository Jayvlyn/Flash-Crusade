using UnityEngine;

public interface ICommandContext :
    IInventoryManager,
    IUINavigator,
    IGridNavigator,
    IPartDestroyer,
    IPartPlacer,
    IPartGrabber,
    IPartTransformer,
    IVisualizer
{ }