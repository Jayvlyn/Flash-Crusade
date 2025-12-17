using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorManager : MonoBehaviour, ICommandContext
{
    [SerializeField] NavItem buildWindow;
    [SerializeField] NavItem exitItem;
    [SerializeField] BuildArea buildArea;

    [SerializeField] NavVisualizer visualizer;
    [SerializeField] UINavigator uiNav;
    [SerializeField] GridNavigator gridNav;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] PartDestroyer partDestroyer;
    [SerializeField] PartPlacer partPlacer;
    [SerializeField] PartGrabber partGrabber;
    [SerializeField] PartTransformer partTransformer;

    EditorState editorState;

    #region Lifecycle

    void OnEnable()
    {
        SubscribeToInputEvents();

        EventBus.Subscribe<EnterInputFieldEvent>(OnEnterInputField);
        EventBus.Subscribe<InventoryPartGrabbedEvent>(OnInventoryPartGrabbedEvent);
    }

    void OnDisable()
    {
        UnsubscribeFromInputEvents();

        EventBus.Unsubscribe<EnterInputFieldEvent>(OnEnterInputField);
        EventBus.Unsubscribe<InventoryPartGrabbedEvent>(OnInventoryPartGrabbedEvent);
    }

    void Awake()
    {
        editorState = new EditorState();

        visualizer.EditorState = editorState;

        uiNav.EditorState = editorState;
        uiNav.gridNav = (IGridNavigator)gridNav;

        gridNav.EditorState = editorState;
        gridNav.uiNav = (IUINavigator)uiNav;

        partDestroyer.EditorState = editorState;
        partDestroyer.grabber = (IPartGrabber) partGrabber;
        partDestroyer.transformer = (IPartTransformer) partTransformer;
        partDestroyer.placer = (IPartPlacer) partPlacer;
        partDestroyer.visualizer = (IVisualizer) visualizer;
        partDestroyer.gridNav = (IGridNavigator) gridNav;
        partDestroyer.inventory = (IInventoryManager) inventoryManager;

        partPlacer.EditorState = editorState;
        partPlacer.buildArea = buildArea;

        partGrabber.EditorState = editorState;
        partGrabber.buildArea = buildArea;
        partGrabber.uiNav = (IUINavigator)uiNav;
        partGrabber.visualizer = (IVisualizer)visualizer;


        partTransformer.EditorState = editorState;
        partTransformer.visualizer = (IVisualizer)visualizer;

        visualizer.gameObject.SetActive(true);
        gridNav.ResetGridPosition();
        uiNav.InitItemMode();
    }

    #endregion

    #region IGridNavigator

    public void InitGridMode() => gridNav.InitGridMode();

    public void TriggerGridNav(Vector2 dir) => gridNav.TriggerGridNav(dir);

    public Vector2Int GetCurrentGridCell() => gridNav.GetCurrentGridCell();

    public void SwitchToItemMode() => gridNav.SwitchToItemMode();

    public void NavToCell(Vector2Int cell) => gridNav.NavToCell(cell);

    public void ResetGridPosition() => gridNav.ResetGridPosition();

    #endregion

    #region IUINavigator

    public void InitItemMode() => uiNav.InitItemMode();

    public void TriggerItemNav(Vector2 dir) => uiNav.TriggerItemNav(dir);

    public void SwitchToGridMode() => uiNav.SwitchToGridMode();
    
    void GoBack()
    {
        if (editorState.inInputField)
        {
            editorState.inInputField = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (editorState.navMode == NavMode.Item)
        {
            if (editorState.HoveredItem == exitItem) exitItem.OnSelected();
            else uiNav.NavToItem(exitItem);
        }
        else if (editorState.navMode == NavMode.Grid)
        {
            CommandHistory.Execute(new ExitGridModeCommand(this, editorState.heldPart));
        }
    }

    void ToggleNavMode()
    {
        if (editorState.navMode == NavMode.Item) CommandHistory.Execute(new EnterGridModeCommand(this));
        else if (editorState.navMode == NavMode.Grid) CommandHistory.Execute(new ExitGridModeCommand(this, editorState.heldPart));
    }

    #endregion

    #region IPartDestroyer

    public void DestroyPart(ShipPart part) => partDestroyer.DestroyPart(part);

    public void HandleUndoRoutine(bool wasPlaced, ShipPartData partData, Vector2Int partPosition, Vector2Int startCell, float rotation, bool xFlipped = false, bool yFlipped = false) =>
        partDestroyer.HandleUndoRoutine(wasPlaced, partData, partPosition, startCell, rotation, xFlipped, yFlipped);

    #endregion

    #region IPartPlacer

    public ShipPart GetHeldPart() => partPlacer.GetHeldPart();

    public void PlacePart(ShipPart part, Vector2Int cell) => partPlacer.PlacePart(part, cell);

    void TryPlacePart()
    {
        if (buildArea.CanPlacePart(editorState.heldPart, gridNav.GetCurrentGridCell()))
        {
            CommandHistory.Execute(new PlaceCommand(this, gridNav.GetCurrentGridCell()));
        }
    }

    bool placeQueued;
    IEnumerator TryPlacePartDelayed()
    {
        if (placeQueued) yield break; // prevents spam stacking
        placeQueued = true;

        if (UIManager.Smoothing && visualizer.IsLerping)
            yield return visualizer.WaitUntilDone();

        TryPlacePart(); // safe now
        placeQueued = false;
    }

    #endregion

    #region IPartGrabber

    public ShipPart GrabFromGrid(Vector2Int cell) => partGrabber.GrabFromGrid(cell);

    public void GrabImmediate(ShipPart part, bool fromInv) => partGrabber.GrabImmediate(part, fromInv);

    public void GrabFrameLate(ShipPart part, bool fromInv) => partGrabber.GrabFrameLate(part, fromInv);

    public void GrabWithLerp(ShipPart part, bool fromInv) => partGrabber.GrabWithLerp(part, fromInv);

    void TryGrabPart()
    {
        ShipPart part = buildArea.GetPartAtCell(gridNav.GetCurrentGridCell());
        if (part) CommandHistory.Execute(new GrabCommand(this, part.position, gridNav.GetCurrentGridCell()));
    }

    #endregion

    #region IInventoryManager

    public bool TryTakePart(ShipPartData data, out ShipPart part) => inventoryManager.TryTakePart(data, out part);

    public void AddPart(ShipPartData data) => inventoryManager.AddPart(data);

    public void SetPartToDefaultStart(ShipPart part) => inventoryManager.SetPartToDefaultStart(part);

    #endregion

    #region IPartTransformer

    public void RotatePart(float angle) => partTransformer.RotatePart(angle);

    public void FlipPart(FlipAxis axis) => partTransformer.FlipPart(axis);

    public void RestorePartTransformations(float rotation, bool xFlipped = false, bool yFlipped = false) =>
        partTransformer.RestorePartTransformations(rotation, xFlipped, yFlipped);

    #endregion

    #region IVisualizer

    public void SetExpanded(bool expanded) => visualizer.SetExpanded(expanded);

    public void HighlightCellImmediate(Vector2Int cell) => visualizer.HighlightCellImmediate(cell);

    public void UpdateWithRectImmediate(RectTransform rect) => visualizer.UpdateWithRectImmediate(rect);

    public void MatchRectScale(RectTransform rect) => visualizer.MatchRectScale(rect);

    public void ResetScale() => visualizer.ResetScale();

    public RectTransform GetRect() => visualizer.GetRect();

    public Coroutine LerpWithRect(RectTransform rt) => visualizer.LerpWithRect(rt);

    public void Flip(FlipAxis axis) => visualizer.Flip(axis);

    public void FlipImmediate(FlipAxis axis) => visualizer.FlipImmediate(axis);

    public void Rotate(float angle) => visualizer.Rotate(angle);

    public void RotateImmediate(float angle) => visualizer.RotateImmediate(angle);

    #endregion

    #region Input Event Handling

    void SubscribeToInputEvents()
    {
        EventBus.Subscribe<SubmitInputEvent>(OnSubmitInputEvent);
        EventBus.Subscribe<CancelInputEvent>(OnCancelInputEvent);
        EventBus.Subscribe<ModeInputEvent>(OnModeInputEvent);
        EventBus.Subscribe<UndoInputEvent>(OnUndoInputEvent);
        EventBus.Subscribe<RedoInputEvent>(OnRedoInputEvent);
        EventBus.Subscribe<DeleteInputEvent>(OnDeleteInputEvent);
        EventBus.Subscribe<ResetInputEvent>(OnResetInputEvent);
        EventBus.Subscribe<NavigateInputEvent>(OnNavigateInputEvent);
        EventBus.Subscribe<ModifyInputEvent>(OnModifyInputEvent);
        EventBus.Subscribe<FlipInputEvent>(OnFlipInputEvent);
        EventBus.Subscribe<RotateInputEvent>(OnRotateInputEvent);
    }

    void UnsubscribeFromInputEvents()
    {
        EventBus.Unsubscribe<SubmitInputEvent>(OnSubmitInputEvent);
        EventBus.Unsubscribe<CancelInputEvent>(OnCancelInputEvent);
        EventBus.Unsubscribe<ModeInputEvent>(OnModeInputEvent);
        EventBus.Unsubscribe<UndoInputEvent>(OnUndoInputEvent);
        EventBus.Unsubscribe<RedoInputEvent>(OnRedoInputEvent);
        EventBus.Unsubscribe<DeleteInputEvent>(OnDeleteInputEvent);
        EventBus.Unsubscribe<ResetInputEvent>(OnResetInputEvent);
        EventBus.Unsubscribe<NavigateInputEvent>(OnNavigateInputEvent);
        EventBus.Unsubscribe<ModifyInputEvent>(OnModifyInputEvent);
        EventBus.Unsubscribe<FlipInputEvent>(OnFlipInputEvent);
        EventBus.Unsubscribe<RotateInputEvent>(OnRotateInputEvent);
    }

    void OnSubmitInputEvent(SubmitInputEvent e)
    {
        if (editorState.navMode == NavMode.Item)
        {
            if (editorState.HoveredItem != null)
            {
                if (editorState.HoveredItem == buildWindow) CommandHistory.Execute(new EnterGridModeCommand(this));
                else editorState.HoveredItem.OnSelected();
            }
        }
        else if (editorState.navMode == NavMode.Grid)
        {
            if (editorState.midGrab) return;
            if (editorState.heldPart != null)
            {
                if (UIManager.Smoothing)
                    StartCoroutine(TryPlacePartDelayed());
                else
                    TryPlacePart();
            }
            else
            {
                TryGrabPart();
            }
        }
    }

    void OnCancelInputEvent(CancelInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || editorState.midUndoDelete)
            return;
        
        GoBack();
    }

    void OnModeInputEvent(ModeInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || editorState.midUndoDelete)
            return;

        ToggleNavMode();
    }

    void OnUndoInputEvent(UndoInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || editorState.midUndoDelete)
            return;

        CommandHistory.Undo();
    }

    void OnRedoInputEvent(RedoInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || editorState.midUndoDelete)
            return;

        CommandHistory.Redo();
    }

    void OnDeleteInputEvent(DeleteInputEvent e)
    {
        if (editorState.navMode != NavMode.Grid) return;
        ShipPart part = buildArea.GetPartAtCell(gridNav.GetCurrentGridCell());
        if (editorState.heldPart == null && part == null) return;

        CommandHistory.Execute(new DeleteCommand(this, gridNav.GetCurrentGridCell()));
    }

    void OnResetInputEvent(ResetInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || editorState.midUndoDelete)
            return;

        if (editorState.navMode == NavMode.Item)
        {
            editorState.HoveredItem = null;
            editorState.LastHoveredItem = null;
            ResetGridPosition();
            uiNav.InitItemMode();
        }
        else if (editorState.navMode == NavMode.Grid)
        {
            CommandHistory.Execute(new ResetCommand(this));
        }
    }

    void OnNavigateInputEvent(NavigateInputEvent e)
    {
        if (editorState.midGrab || ZoomController.MidZoom) return;

        Vector2 dir = e.dir;

        dir.x = Mathf.RoundToInt(dir.x);
        dir.y = Mathf.RoundToInt(dir.y);
        if (modifyHeld)
        {
            dir.x *= 3;
            dir.y *= 3;
        }

        if (editorState.navMode == NavMode.Grid) CommandHistory.Execute(new NavigateCommand(this, dir, editorState));
        else
        {
            uiNav.TriggerItemNav(dir);
            if (modifyHeld) uiNav.TriggerItemNav(dir); // double trigger when modify held (dir mag doesnt matter for item mode)
        }
    }

    bool modifyHeld;
    void OnModifyInputEvent(ModifyInputEvent e) => modifyHeld = e.held;

    void OnFlipInputEvent(FlipInputEvent e)
    {
        if (editorState.heldPart == null) return;
        if (editorState.navMode != NavMode.Grid) return;
        if (visualizer.IsFlipLerping) return;

        CommandHistory.Execute(new FlipCommand(this, e.flipAxis));
    }

    void OnRotateInputEvent(RotateInputEvent e)
    {
        if (editorState.heldPart == null) return;
        if (editorState.navMode != NavMode.Grid) return;
        if (visualizer.IsRotateLerping) return;

        float angle = 0;
        if (e.rotationDirection == RotationDirection.Clockwise) angle = 90;
        else angle = -90;

        if (modifyHeld) angle *= 1.999f; // comes out to ~179.9 so that lerp happens in correct direction, will snap to int later
        CommandHistory.Execute(new RotateCommand(this, angle));
    }

    void OnEnterInputField(EnterInputFieldEvent e) => editorState.inInputField = true;

    public void OnInventoryPartGrabbedEvent(InventoryPartGrabbedEvent e) => CommandHistory.Execute(new InventoryGrabCommand(this, e.part));

    #endregion
}