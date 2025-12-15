using UnityEngine;

public struct SubmitInputEvent { }
public struct CancelInputEvent { }
public struct ModeInputEvent { }
public struct UndoInputEvent { }
public struct RedoInputEvent { }
public struct DeleteInputEvent { }
public struct ResetInputEvent { }
public struct NavigateInputEvent { public Vector2 dir; }
public struct ModifyInputEvent { public bool held; }
public struct FlipInputEvent { public FlipAxis flipAxis; }
public struct ZoomInputEvent { public ZoomDirection zoomDirection; }
public struct RotateInputEvent { public RotationDirection rotationDirection; }
