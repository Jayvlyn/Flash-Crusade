using UnityEngine;

public interface ICommandContext :
    IInventoryManager,
    INavigator,
    IPartDestroyer,
    IPartPlacer,
    IPartGrabber,
    IPartTransformer,
    IVisualizer
{ }