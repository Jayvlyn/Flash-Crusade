using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavManager : MonoBehaviour, IEditorCommandContext
{
    [SerializeField] Vector2 zoomRange = new Vector2(1, 10);
    int zoomLevel = 3;
    public int ZoomLevel
    {
        get => zoomLevel;
        set => zoomLevel = (int)Mathf.Clamp(value, zoomRange.x, zoomRange.y);
    }
    Dictionary<int,float> zoomScales = new Dictionary<int,float>();

    public Vector2Int currentGridCell = new Vector2Int(0,0); // temp public
    public enum NavMode { Item, Grid };
    [HideInInspector] public NavMode mode = NavMode.Item;
    bool Expanded => heldPart != null;

    public NavItem buildWindow;
    [SerializeField] RectTransform centerGridCell;
    [SerializeField] public NavVisualizer visualizer; // temp public
    [SerializeField] NavItem initialHoveredItem;
    [SerializeField] NavItem exitItem;
    [SerializeField] public EditorBuildArea buildArea; // temp public
    [HideInInspector] public PartOrganizer partOrganizer;
    NavItem hoveredItem;
    NavItem lastHoveredItem;
    NavItem HoveredItem
    {
        get => hoveredItem;
        set
        {
            if (hoveredItem == value) return;
            lastHoveredItem = hoveredItem;
            hoveredItem = value;
        }
    }
    public EditorShipPart heldPart; // temp public

    bool modifyHeld;

    void OnEnable()
    {
        SubscribeToInputEvents();
        visualizer.gameObject.SetActive(true);

        EventBus.Subscribe<EnterInputFieldEvent>(OnEnterInputField);
    }

    void OnDisable()
    {
        UnsubscribeFromInputEvents();
        visualizer.gameObject.SetActive(false);

        EventBus.Unsubscribe<EnterInputFieldEvent>(OnEnterInputField);
    }

    void Awake()
    {
        // based on 16:9 ratio
        zoomScales.Add(1, 1.48f);
        zoomScales.Add(2, 0.89f);
        zoomScales.Add(3, 0.635f);
        zoomScales.Add(4, 0.493f);
        zoomScales.Add(5, 0.403f);
        zoomScales.Add(6, 0.3415f);
        zoomScales.Add(7, 0.29597f);
        zoomScales.Add(8, 0.26114f);
        zoomScales.Add(9, 0.23365f);
        zoomScales.Add(10,0.21141f);
    }

    void Start()
    {
        visualizer.centerGridCell = centerGridCell;

        if (buildArea == null) buildArea = FindFirstObjectByType<EditorBuildArea>();
        if (visualizer == null) visualizer = FindFirstObjectByType<NavVisualizer>();

        ResetGridPosition();
        InitNavMode();
    }

    private Coroutine zoomRoutine;
    private Vector3 targetZoomScale;
    void OnNewZoomLevel()
    {
        float s = zoomScales[zoomLevel];
        targetZoomScale = new Vector3(s, s, s);

        if (UIManager.Smoothing)
        {
            if (zoomRoutine != null) StopCoroutine(zoomRoutine);
            zoomRoutine = StartCoroutine(LerpZoom(targetZoomScale));
        }
        else
        {
            buildArea.transform.localScale = new Vector3(s, s, s);
            if (mode == NavMode.Grid) visualizer.HighlightCellImmediate(currentGridCell, Expanded);
        }
    }

    private bool midZoom;
    IEnumerator LerpZoom(Vector3 target, float duration = 0.15f)
    {
        midZoom = true;
        float t = 0f;
        Vector3 start = buildArea.transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            buildArea.transform.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            if (mode == NavMode.Grid) visualizer.HighlightCellImmediate(currentGridCell, Expanded);

            yield return null;
        }
        buildArea.transform.localScale = target;
        midZoom = false;
    }

    #region INavigator

    public Vector2Int GetCurrentGridCell() => currentGridCell;

    public void SwitchToItemMode() => SwitchNavMode(NavMode.Item);

    public void SwitchToGridMode() => SwitchNavMode(NavMode.Grid);

    public void InitNavMode()
    {
        switch (mode)
        {
            case NavMode.Item:
                NavItem targetItem = null;
                if (lastHoveredItem != null) targetItem = lastHoveredItem;
                else if (initialHoveredItem != null) targetItem = initialHoveredItem;
                else targetItem = GetComponentInChildren<NavItem>();
                NavToItem(targetItem);

                visualizer.ResetRotation();
                visualizer.ResetScale();
                break;

            case NavMode.Grid:
                HoveredItem = null;
                NavToCell(currentGridCell);
                break;
        }
    }

    public void TriggerNav(Vector2 dir)
    {
        if (mode == NavMode.Item)
        {
            TriggerNavItem(dir);
            return;
        }

        if (mode == NavMode.Grid)
        {
            TriggerNavGrid(dir);
            return;
        }
    }

    void TriggerNavItem(Vector2 dir)
    {
        if (HoveredItem == null)
            return;

        NavItem next = null;

        if (dir.y > 0.5f) next = HoveredItem.navUp;
        else if (dir.y < -0.5f) next = HoveredItem.navDown;
        else if (dir.x < -0.5f) next = HoveredItem.navLeft;
        else if (dir.x > 0.5f) next = HoveredItem.navRight;

        if (next == null)
            return;

        NavToItem(next);
    }

    void TriggerNavGrid(Vector2 dir)
    {
        Vector2Int offset = new Vector2Int((int)dir.x, (int)dir.y);

        if (offset == Vector2Int.zero)
            return;

        Vector2Int newCell = currentGridCell + offset;

        NavToCell(newCell);
    }

    void NavToItem(NavItem item)
    {
        if (item == null) return;
        HoveredItem = item;
        HoveredItem.OnHighlighted();
        visualizer.HighlightItem(HoveredItem);
    }

    public void NavToCell(Vector2Int cell)
    {
        currentGridCell = cell;
        visualizer.HighlightCell(currentGridCell, Expanded);
    }

    public void ResetGridPosition()
    {
        currentGridCell = Vector2Int.zero;
    }

    void SwitchNavMode(NavMode newMode)
    {
        if (mode == newMode) return;
        mode = newMode;
        InitNavMode();
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
            if (HoveredItem == exitItem) exitItem.OnSelected();
            else NavToItem(exitItem);
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
            NavToCell(startCell);
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
        bool horizontal = axis == FlipAxis.Horizontal;
        if (heldPart.Rotation == 90 || heldPart.Rotation == 270) horizontal = !horizontal;
        heldPart.Flip(horizontal);
        visualizer.Flip(horizontal);
    }

    public void RestorePartTransformations(float rotation, bool xFlipped = false, bool yFlipped = false) // temp public
    {
        if (xFlipped) FlipPartImmediate(FlipAxis.Horizontal);
        if (yFlipped) FlipPartImmediate(FlipAxis.Vertical);
        if (rotation != 0) RotatePartImmediate(rotation);
    }

    void FlipPartImmediate(FlipAxis axis)
    {
        bool horizontal = axis == FlipAxis.Horizontal;
        if (heldPart.Rotation == 90 || heldPart.Rotation == 270) horizontal = !horizontal;
        heldPart.Flip(horizontal);
        visualizer.FlipImmediate(horizontal);
    }

    void RotatePartImmediate(float angle)
    {
        heldPart.Rotate(angle);
        visualizer.RotateImmediate(angle);
    }

    #endregion

    #region IVisualizer

    public void HighlightCellImmediate(Vector2Int cell, bool expanded = false)
    {
        visualizer.HighlightCellImmediate(cell, expanded);
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
        if (buildArea.CanPlacePart(heldPart, currentGridCell))
        {
            CommandHistory.Execute(new PlaceCommand(this, currentGridCell));
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
        if(!fromInv) currentGridCell = part.position;
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
        EditorShipPart part = buildArea.GetPartAtCell(currentGridCell);
        if (part) CommandHistory.Execute(new GrabCommand(this, part.position, currentGridCell));
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
        if (!fromInv) currentGridCell = part.position;
        heldPart = part;
        midGrab = false;
        if (fromInv) SwitchToGridMode();
    }

    public void OnInventoryPartGrabbed(ShipPartData part)
    {
        CommandHistory.Execute(new InventoryGrabCommand(this, part));
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
        EventBus.Subscribe<ZoomInputEvent>(OnZoomInputEvent);
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
        EventBus.Unsubscribe<ZoomInputEvent>(OnZoomInputEvent);
        EventBus.Unsubscribe<RotateInputEvent>(OnRotateInputEvent);
    }

    void OnSubmitInputEvent(SubmitInputEvent e)
    {
        // submit is disabled when in input field, no need to consider that case
        if (mode == NavMode.Item)
        {
            if (HoveredItem != null)
            {
                if (HoveredItem == buildWindow) CommandHistory.Execute(new EnterGridModeCommand(this));
                else HoveredItem.OnSelected();
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
        EditorShipPart part = buildArea.GetPartAtCell(currentGridCell);
        if (heldPart == null && part == null) return;

        CommandHistory.Execute(new DeleteCommand(this, currentGridCell));
    }

    void OnResetInputEvent(ResetInputEvent e)
    {
        if (mode == NavMode.Item)
        {
            HoveredItem = null;
            lastHoveredItem = null;
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
        if (midGrab || midZoom) return;

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

    void OnZoomInputEvent(ZoomInputEvent e)
    {
        ZoomDirection zoomDir = e.zoomDirection;
        if      (zoomDir == ZoomDirection.In)  ZoomLevel--;
        else if (zoomDir == ZoomDirection.Out) ZoomLevel++;
        OnNewZoomLevel();
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