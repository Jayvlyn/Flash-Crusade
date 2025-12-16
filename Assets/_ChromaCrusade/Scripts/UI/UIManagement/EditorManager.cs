using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorManager : MonoBehaviour, IEditorCommandContext
{
    [SerializeField] NavItem buildWindow;
    [SerializeField] NavVisualizer visualizer;
    [SerializeField] NavItem exitItem;
    [SerializeField] EditorBuildArea buildArea;
    [SerializeField] PartOrganizer partOrganizer;
    [SerializeField] GridNavigator gridNav;
    [SerializeField] UINavigator uiNav;

    public enum NavMode { Item, Grid };
    NavMode mode = NavMode.Item;
    EditorShipPart heldPart;

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

    void Start()
    {
        if (buildArea == null) buildArea = FindFirstObjectByType<EditorBuildArea>();
        if (visualizer == null) visualizer = FindFirstObjectByType<NavVisualizer>();
        if (gridNav == null) gridNav = FindFirstObjectByType<GridNavigator>();
        if (uiNav == null) uiNav = FindFirstObjectByType<UINavigator>();
        gridNav.ResetGridPosition();
        InitNavMode();
    }

    #region INavigator

    public Vector2Int GetCurrentGridCell() => gridNav.CurrentGridCell;

    void SwitchNavMode(NavMode newMode)
    {
        if (mode == newMode) return;
        mode = newMode;
        InitNavMode();
    }

    public void SwitchToItemMode() => SwitchNavMode(NavMode.Item);

    public void SwitchToGridMode() => SwitchNavMode(NavMode.Grid);

    public void NavToCell(Vector2Int cell)
    {
        gridNav.NavToCell(cell);
    }

    public void ResetGridPosition()
    {
        gridNav.ResetGridPosition();
    }

    public void InitNavMode()
    {
        switch (mode)
        {
            case NavMode.Item:
                uiNav.Init();
                break;

            case NavMode.Grid:
                uiNav.HoveredItem = null;
                gridNav.Init();
                break;
        }
    }

    public void TriggerNav(Vector2 dir)
    {
        if (mode == NavMode.Item)
        {
            uiNav.TriggerNav(dir);
            return;
        }

        if (mode == NavMode.Grid)
        {
            gridNav.TriggerNav(dir);
            return;
        }
    }

    bool inInputField;
    void GoBack()
    {
        if (inInputField)
        {
            inInputField = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (mode == NavMode.Item)
        {
            if (uiNav.HoveredItem == exitItem) exitItem.OnSelected();
            else uiNav.NavToItem(exitItem);
        }
        else if (mode == NavMode.Grid)
        {
            CommandHistory.Execute(new ExitGridModeCommand(this, heldPart));
        }
    }

    void ToggleNavMode()
    {
        if (mode == NavMode.Item) CommandHistory.Execute(new EnterGridModeCommand(this));
        else if (mode == NavMode.Grid) CommandHistory.Execute(new ExitGridModeCommand(this, heldPart));
    }

    #endregion

    #region IPartDestroyer

    public void DestroyPart(EditorShipPart part)
    {
        if (part == heldPart) heldPart = null;
        Destroy(part.gameObject);
    }

    public void HandleUndoRoutine(bool wasPlaced, ShipPartData partData, Vector2Int partPosition, Vector2Int startCell, float rotation, bool xFlipped = false, bool yFlipped = false)
    {
        StartCoroutine(UndoDeleteRoutine(wasPlaced,partData, partPosition, startCell, rotation, xFlipped, yFlipped));
    }

    bool midUndoDelete;
    IEnumerator UndoDeleteRoutine(bool wasPlaced, ShipPartData partData, Vector2Int partPosition, Vector2Int startCell, float rotation, bool xFlipped = false, bool yFlipped = false)
    {
        midUndoDelete = true;
        bool success = partOrganizer.TryTakePart(partData, out EditorShipPart part);
        if (success) GrabImmediate(part, true);
        yield return null;
        RestorePartTransformations(rotation, xFlipped, yFlipped);
        yield return null;

        if (wasPlaced)
        {
            buildArea.PlacePart(part, partPosition);
            part.OnPlaced(partPosition, buildArea);
            heldPart = null;
            visualizer.ResetScale();
            SetExpanded(false);
            NavToCell(startCell);
        }
        else
        {
            SetExpanded(true);
        }
        midUndoDelete = false;
    }

    #endregion

    #region IInventoryManager

    public bool TryTakePart(ShipPartData data, out EditorShipPart part)
    {
        return partOrganizer.TryTakePart(data, out part);
    }

    public void AddPart(ShipPartData data)
    {
        partOrganizer.AddPart(data);
    }

    public void SetPartToDefaultStart(EditorShipPart part)
    {
        partOrganizer.SetPartToDefaultStart(part);
    }

    #endregion

    #region IPartTransformer

    public void RotatePart(float angle)
    {
        heldPart.Rotate(angle);
        visualizer.Rotate(angle);
    }

    public void FlipPart(FlipAxis axis)
    {
        if (heldPart.Rotation == 90 || heldPart.Rotation == 270)
        {
            if (axis == FlipAxis.Horizontal) axis = FlipAxis.Vertical;
            else axis = FlipAxis.Horizontal;
        }
        heldPart.Flip(axis);
        visualizer.Flip(axis);
    }

    public void RestorePartTransformations(float rotation, bool xFlipped = false, bool yFlipped = false) // temp public
    {
        if (xFlipped) FlipPartImmediate(FlipAxis.Horizontal);
        if (yFlipped) FlipPartImmediate(FlipAxis.Vertical);
        if (rotation != 0) RotatePartImmediate(rotation);
    }

    void FlipPartImmediate(FlipAxis axis)
    {
        if (heldPart.Rotation == 90 || heldPart.Rotation == 270)
        {
            if (axis == FlipAxis.Horizontal) axis = FlipAxis.Vertical;
            else                             axis = FlipAxis.Horizontal;
        }
        heldPart.Flip(axis);
        visualizer.FlipImmediate(axis);
    }

    void RotatePartImmediate(float angle)
    {
        heldPart.Rotate(angle);
        visualizer.RotateImmediate(angle);
    }

    #endregion

    #region IVisualizer

    public void SetExpanded(bool expanded)
    {
        visualizer.expanded = expanded;
    }

    public void HighlightCellImmediate(Vector2Int cell)
    {
        visualizer.HighlightCellImmediate(cell);
    }

    public void UpdateWithRectImmediate(RectTransform rect)
    {
        visualizer.UpdateWithRectImmediate(rect);
    }

    public void MatchRectScale(RectTransform rect)
    {
        visualizer.MatchRectScale(rect);
    }

    public void ResetScale()
    {
        visualizer.ResetScale();
    }

    #endregion

    #region IPartPlacer

    public void PlacePart(EditorShipPart part, Vector2Int cell)
    {
        buildArea.PlacePart(part, cell);
        part.OnPlaced(cell, buildArea);
        heldPart = null;
    }

    public EditorShipPart GetHeldPart()
    {
        return heldPart;
    }

    void TryPlacePart()
    {
        if (buildArea.CanPlacePart(heldPart, gridNav.CurrentGridCell))
        {
            CommandHistory.Execute(new PlaceCommand(this, gridNav.CurrentGridCell));
        }
    }

    private bool placeQueued;
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

    public EditorShipPart GrabFromGrid(Vector2Int cell)
    {
        return buildArea.GrabPart(cell);
    }

    public void GrabImmediate(EditorShipPart part, bool fromInv)
    {
        part.OnGrabbed(visualizer.rect);
        if(!fromInv) gridNav.CurrentGridCell = part.position;
        heldPart = part;
    }

    public void GrabFrameLate(EditorShipPart part, bool fromInv)
    {
        heldPart = part;
        StartCoroutine(GrabFrameLateRoutine(part, fromInv));
    }

    public void GrabWithLerp(EditorShipPart part, bool fromInv)
    {
        StartCoroutine(GrabWithLerpRoutine(part, fromInv));
    }


    void TryGrabPart()
    {
        EditorShipPart part = buildArea.GetPartAtCell(gridNav.CurrentGridCell);
        if (part) CommandHistory.Execute(new GrabCommand(this, part.position, gridNav.CurrentGridCell));
    }

    IEnumerator GrabFrameLateRoutine(EditorShipPart part, bool fromInv)
    {
        yield return null;
        visualizer.UpdateWithRectImmediate(part.rect);
        GrabImmediate(part, fromInv);
    }

    private bool midGrab = false;
    IEnumerator GrabWithLerpRoutine(EditorShipPart part, bool fromInv)
    {
        midGrab = true;
        yield return visualizer.LerpWithRect(part.rect); // waits until done

        part.OnGrabbed(visualizer.rect);
        if (!fromInv) gridNav.CurrentGridCell = part.position;
        heldPart = part;
        midGrab = false;
        if (fromInv) SwitchToGridMode();
    }

    public void OnInventoryPartGrabbedEvent(InventoryPartGrabbedEvent e)
    {
        CommandHistory.Execute(new InventoryGrabCommand(this, e.part));
    }

    #endregion

    #region TEMP INPUT EVENT HANDLING

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
        // submit is disabled when in input field, no need to consider that case
        if (mode == NavMode.Item)
        {
            if (uiNav.HoveredItem != null)
            {
                if (uiNav.HoveredItem == buildWindow) CommandHistory.Execute(new EnterGridModeCommand(this));
                else uiNav.HoveredItem.OnSelected();
            }
        }
        else if (mode == NavMode.Grid)
        {
            if (midGrab) return;
            if (heldPart != null)
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
        GoBack();
    }

    void OnModeInputEvent(ModeInputEvent e)
    {
        ToggleNavMode();
    }

    void OnUndoInputEvent(UndoInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || midUndoDelete)
            return;

        CommandHistory.Undo();
    }

    void OnRedoInputEvent(RedoInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || midUndoDelete)
            return;

        CommandHistory.Redo();
    }

    void OnDeleteInputEvent(DeleteInputEvent e)
    {
        if (mode != NavMode.Grid) return;
        EditorShipPart part = buildArea.GetPartAtCell(gridNav.CurrentGridCell);
        if (heldPart == null && part == null) return;

        CommandHistory.Execute(new DeleteCommand(this, gridNav.CurrentGridCell));
    }

    void OnResetInputEvent(ResetInputEvent e)
    {
        if (mode == NavMode.Item)
        {
            uiNav.HoveredItem = null;
            uiNav.LastHoveredItem = null;
            ResetGridPosition();
            InitNavMode();
        }
        else if (mode == NavMode.Grid)
        {
            CommandHistory.Execute(new ResetCommand(this));
        }
    }

    void OnNavigateInputEvent(NavigateInputEvent e)
    {
        if (midGrab || EditorZoomController.MidZoom) return;

        Vector2 dir = e.dir;

        dir.x = Mathf.RoundToInt(dir.x);
        dir.y = Mathf.RoundToInt(dir.y);
        if (modifyHeld)
        {
            dir.x *= 3;
            dir.y *= 3;
        }

        if (mode == NavMode.Grid) CommandHistory.Execute(new NavigateCommand(this, dir));
        else
        {
            TriggerNav(dir);
            if (modifyHeld) TriggerNav(dir); // double trigger when modify held (dir mag doesnt matter for item mode)
        }
    }


    bool modifyHeld;
    void OnModifyInputEvent(ModifyInputEvent e) 
    {
        modifyHeld = e.held;
    }

    void OnFlipInputEvent(FlipInputEvent e)
    {
        if (heldPart == null) return;
        if (mode != NavMode.Grid) return;
        if (visualizer.IsFlipLerping) return;

        CommandHistory.Execute(new FlipCommand(this, e.flipAxis));
    }

    void OnRotateInputEvent(RotateInputEvent e)
    {
        if (heldPart == null) return;
        if (mode != NavMode.Grid) return;
        if (visualizer.IsRotateLerping) return;

        float angle = 0;
        if (e.rotationDirection == RotationDirection.Clockwise) angle = 90;
        else angle = -90;

        if (modifyHeld) angle *= 1.999f; // comes out to ~179.9 so that lerp happens in correct direction, will snap to int later
        CommandHistory.Execute(new RotateCommand(this, angle));
    }

    void OnEnterInputField(EnterInputFieldEvent e)
    {
        inInputField = true;
    }

    #endregion
}