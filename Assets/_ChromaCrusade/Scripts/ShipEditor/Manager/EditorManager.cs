using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorManager : MonoBehaviour, ICommandContext
{
    [SerializeField] NavItem buildWindow;
    [SerializeField] NavItem exitItem;
    [SerializeField] BuildArea buildArea;
    [SerializeField] ShipNameValidator nameValidator;
    [SerializeField] TMP_Text validationResponseText;

    [SerializeField] EditorNavVisualizer visualizer;
    [SerializeField] EditorUINavigator uiNav;
    [SerializeField] EditorGridNavigator gridNav;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] PartDestroyer partDestroyer;
    [SerializeField] PartPlacer partPlacer;
    [SerializeField] PartGrabber partGrabber;
    [SerializeField] PartTransformer partTransformer;

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
        EditorState.Init();

        uiNav.gridNav = (IGridNavigator)gridNav;

        gridNav.uiNav = (IUINavigator)uiNav;

        partDestroyer.grabber = (IPartGrabber) partGrabber;
        partDestroyer.transformer = (IPartTransformer) partTransformer;
        partDestroyer.placer = (IPartPlacer) partPlacer;
        partDestroyer.visualizer = (IEditorNavVisualizer) visualizer;
        partDestroyer.gridNav = (IGridNavigator) gridNav;
        partDestroyer.inventory = (IInventoryManager) inventoryManager;

        partPlacer.buildArea = buildArea;

        partGrabber.buildArea = buildArea;
        partGrabber.uiNav = (IUINavigator)uiNav;
        partGrabber.visualizer = (IEditorNavVisualizer)visualizer;

        partTransformer.visualizer = (IEditorNavVisualizer)visualizer;

        visualizer.gameObject.SetActive(true);
        gridNav.ResetGridPosition();
        uiNav.Init();
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

    public void Init() => uiNav.Init();

    public void TriggerItemNav(Vector2 dir) => uiNav.TriggerItemNav(dir);

    public void SwitchOff() => uiNav.SwitchOff();
    
    void GoBack()
    {
        if (uiNav.NavState.inInputField)
        {
            uiNav.NavState.inInputField = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (EditorState.navMode == NavMode.Item)
        {
            if (uiNav.NavState.HoveredItem == exitItem) exitItem.OnSelected();
            else uiNav.NavToItem(exitItem);
        }
        else if (EditorState.navMode == NavMode.Grid)
        {
            CommandHistory.Execute(new ExitGridModeCommand(this, EditorState.heldPart));
        }
    }

    void ToggleNavMode()
    {
        if (EditorState.navMode == NavMode.Item) CommandHistory.Execute(new EnterGridModeCommand(this));
        else if (EditorState.navMode == NavMode.Grid) CommandHistory.Execute(new ExitGridModeCommand(this, EditorState.heldPart));
    }

    public void NavToItem(NavItem item) => uiNav.NavToItem(item);

    public void HighlightItem(NavItem newItem) => visualizer.HighlightItem(newItem);

    public void HighlightItemImmediate() => visualizer.HighlightItemImmediate();

    public void HighlightItemLerp() => visualizer.HighlightItemLerp();

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
        if (buildArea.CanPlacePart(EditorState.heldPart, gridNav.GetCurrentGridCell()))
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
        if (EditorState.navMode == NavMode.Item)
        {
            if (uiNav.NavState.HoveredItem != null)
            {
                if (uiNav.NavState.HoveredItem == buildWindow) CommandHistory.Execute(new EnterGridModeCommand(this));
                else uiNav.NavState.HoveredItem.OnSelected();
            }
        }
        else if (EditorState.navMode == NavMode.Grid)
        {
            if (EditorState.midGrab) return;
            if (EditorState.heldPart != null)
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
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || EditorState.midUndoDelete)
            return;
        
        GoBack();
    }

    void OnModeInputEvent(ModeInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || EditorState.midUndoDelete)
            return;

        ToggleNavMode();
    }

    void OnUndoInputEvent(UndoInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || EditorState.midUndoDelete)
            return;

        CommandHistory.Undo();
    }

    void OnRedoInputEvent(RedoInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || EditorState.midUndoDelete)
            return;

        CommandHistory.Redo();
    }

    void OnDeleteInputEvent(DeleteInputEvent e)
    {
        if (EditorState.navMode != NavMode.Grid) return;
        ShipPart part = buildArea.GetPartAtCell(gridNav.GetCurrentGridCell());
        if (EditorState.heldPart == null && part == null) return;

        CommandHistory.Execute(new DeleteCommand(this, gridNav.GetCurrentGridCell()));
    }

    void OnResetInputEvent(ResetInputEvent e)
    {
        if (visualizer.IsRotateLerping || visualizer.IsFlipLerping || visualizer.IsLerping || EditorState.midUndoDelete)
            return;

        if (EditorState.navMode == NavMode.Item)
        {
            uiNav.NavState.HoveredItem = null;
            uiNav.NavState.LastHoveredItem = null;
            ResetGridPosition();
            uiNav.Init();
        }
        else if (EditorState.navMode == NavMode.Grid)
        {
            CommandHistory.Execute(new ResetCommand(this));
        }
    }

    void OnNavigateInputEvent(NavigateInputEvent e)
    {
        if (EditorState.midGrab || ZoomController.MidZoom) return;
        if (InventoryManager.Scrolling) return;

        Vector2 dir = e.dir;

        dir.x = Mathf.RoundToInt(dir.x);
        dir.y = Mathf.RoundToInt(dir.y);
        if (modifyHeld)
        {
            dir.x *= 3;
            dir.y *= 3;
        }

        if (EditorState.navMode == NavMode.Grid) CommandHistory.Execute(new NavigateCommand(this, dir));
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
        if (EditorState.heldPart == null) return;
        if (EditorState.navMode != NavMode.Grid) return;
        if (visualizer.IsFlipLerping) return;

        CommandHistory.Execute(new FlipCommand(this, e.flipAxis));
    }

    void OnRotateInputEvent(RotateInputEvent e)
    {
        if (EditorState.heldPart == null) return;
        if (EditorState.navMode != NavMode.Grid) return;
        if (visualizer.IsRotateLerping) return;

        float angle = 0;
        if (e.rotationDirection == RotationDirection.Clockwise) angle = 90;
        else angle = -90;

        if (modifyHeld) angle *= 1.999f; // comes out to ~179.9 so that lerp happens in correct direction, will snap to int later
        CommandHistory.Execute(new RotateCommand(this, angle));
    }

    void OnEnterInputField(EnterInputFieldEvent e) => uiNav.NavState.inInputField = true;

    void OnInventoryPartGrabbedEvent(InventoryPartGrabbedEvent e) => CommandHistory.Execute(new InventoryGrabCommand(this, e.part));

    public void OnExitButtonSelected()
    {
        Debug.Log("exit pressed");
    }

    public void OnCompleteButtonSelected()
    {
        ShipBuildValidator validator = new ShipBuildValidator(buildArea, nameValidator);

        string result = validator.ValidateCurrentBuild();

        if(result.Equals("Valid"))
        {
            ShipSaveLoader ShipSL = new ShipSaveLoader(buildArea);
            ShipSL.SaveCurrentBuild(nameValidator.GetName());
        }
        else
        {
            SetResponseText(result);
        }
    }

    #endregion

    #region Validation Response

    void SetResponseText(string content, float duration = 5)
    {
        validationResponseText.text = content;
        if(validationTextClearerCoroutine != null) StopCoroutine(validationTextClearerCoroutine);
        validationTextClearerCoroutine = StartCoroutine(ValidationResponseTextClearer(duration));
    }

    Coroutine validationTextClearerCoroutine;
    IEnumerator ValidationResponseTextClearer(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        validationResponseText.text = "";
    }

    #endregion
}