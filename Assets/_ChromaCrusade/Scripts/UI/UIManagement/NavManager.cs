using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static EditorInputManager;

public class NavManager : MonoBehaviour
{
    [SerializeField] Vector2 zoomRange = new Vector2(1, 10);
    int zoomLevel = 3;
    public int ZoomLevel
    {
        get => zoomLevel;
        set => zoomLevel = (int)Mathf.Clamp(value, zoomRange.x, zoomRange.y);
    }
    Dictionary<int,float> zoomScales = new Dictionary<int,float>();

    Vector2Int currentGridCell = new Vector2Int(0,0);
    public enum NavMode { Item, Grid };
    [HideInInspector] public NavMode mode = NavMode.Item;
    bool Expanded => heldPart != null;

    public NavItem buildWindow;
    [SerializeField] RectTransform centerGridCell;
    [SerializeField] NavVisualizer visualizer;
    [SerializeField] NavItem initialHoveredItem;
    [SerializeField] NavItem exitItem;
    [SerializeField] EditorBuildArea buildArea;
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
    private EditorShipPart heldPart;

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

        InitNavMode(true);
    }

    void TriggerNav(Vector2 dir)
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
        if(item == null) return;
        HoveredItem = item;
        HoveredItem.OnHighlighted();
        visualizer.HighlightItem(HoveredItem);
    }

    void NavToCell(Vector2Int cell)
    {
        currentGridCell = cell;
        visualizer.HighlightCell(currentGridCell, Expanded);
    }

    public void SwitchNavMode(NavMode newMode)
    {
        if (mode == newMode) return;
        mode = newMode;
        InitNavMode(false);
    }

    public void SwitchToItemMode() => SwitchNavMode(NavMode.Item);

    public void SwitchToGridMode() => SwitchNavMode(NavMode.Grid);

    void InitNavMode(bool resetGrid)
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
                if (resetGrid) currentGridCell = Vector2Int.zero;
                HoveredItem = null;
                NavToCell(currentGridCell);
                break;
        }
    }

    void TryGrabPart()
    {
        EditorShipPart part = buildArea.GetPartAtCell(currentGridCell);
        if (part)
        {
            CommandHistory.Execute(new GrabCommand(this, part));
        }
    }

    void GrabFrameLate(EditorShipPart part, bool fromInv)
    {
        heldPart = part;
        StartCoroutine(FrameLateRoutine(part, fromInv));
    }

    IEnumerator FrameLateRoutine(EditorShipPart part, bool fromInv)
    {
        yield return null;
        GrabImmediate(part, fromInv);
    }

    void GrabImmediate(EditorShipPart part, bool fromInv, bool updateVisualizer = true)
    {
        if(updateVisualizer) visualizer.UpdateWithRectImmediate(part.rect);
        part.OnGrabbed(visualizer.rect);
        if(!fromInv) currentGridCell = part.position;
        heldPart = part;
    }

    private bool midGrab = false;
    IEnumerator GrabWithLerp(EditorShipPart part, bool fromInv)
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

    void TryPlacePart()
    {
        if(buildArea.CanPlacePart(currentGridCell, heldPart))
        {
            CommandHistory.Execute(new PlaceCommand(this, currentGridCell));
        }
    }

    private bool placeQueued;
    IEnumerator TryPlaceWithSync()
    {
        if (placeQueued) yield break; // prevents spam stacking
        placeQueued = true;

        if (UIManager.Smoothing && visualizer.IsLerping)
            yield return visualizer.WaitUntilDone();

        TryPlacePart(); // safe now
        placeQueued = false;
    }


    public void RotatePart(float angle)
    {
        heldPart.Rotate(angle);
        visualizer.Rotate(angle);
    }

    public void RotatePartImmediate(float angle)
    {
        heldPart.Rotate(angle);
        visualizer.RotateImmediate(angle);
    }

    public void FlipPart(FlipAxis axis)
    {
        bool horizontal = axis == FlipAxis.Horizontal;
        if (heldPart.Rotation == 90 || heldPart.Rotation == 270) horizontal = !horizontal;
        heldPart.Flip(horizontal);
        visualizer.Flip(horizontal);
    }

    public void FlipPartImmediate(FlipAxis axis)
    {
        bool horizontal = axis == FlipAxis.Horizontal;
        if (heldPart.Rotation == 90 || heldPart.Rotation == 270) horizontal = !horizontal;
        heldPart.Flip(horizontal);
        visualizer.FlipImmediate(horizontal);
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

    bool inInputField;
    private void GoBack()
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
            CommandHistory.Execute(new ExitGridModeCommand(this));
        }
    }


    void ToggleNavMode()
    {
        if (mode == NavMode.Item) CommandHistory.Execute(new EnterGridModeCommand(this));
        else if (mode == NavMode.Grid) CommandHistory.Execute(new ExitGridModeCommand(this));
    }

    bool midUndoDelete;
    IEnumerator UndoDeleteRoutine(bool wasPlaced, ShipPartData partData, Vector2Int partPosition, Vector2Int startCell, float rotation, bool xFlipped = false, bool yFlipped = false)
    {
        bool success = partOrganizer.TryTakePart(partData, out EditorShipPart part);
        //partOrganizer.SetPartToDefaultStart(part);
        if (success) GrabImmediate(part, true, false);
        yield return null;
        RestorePartTransformations(rotation, xFlipped, yFlipped);
        yield return null;

        if (wasPlaced)
        {
            buildArea.PlacePart(partPosition, part);
            part.OnPlaced(partPosition, buildArea);
            heldPart = null;
            visualizer.ResetScale();
            visualizer.HighlightCellImmediate(startCell, false);
        }
        midUndoDelete = false;
    }

    void RestorePartTransformations(float rotation, bool xFlipped = false, bool yFlipped = false)
    {
        if (rotation != 0) RotatePartImmediate(rotation);
        if (xFlipped) FlipPartImmediate(FlipAxis.Horizontal);
        if (yFlipped) FlipPartImmediate(FlipAxis.Vertical);
    }





    #region TEMP COMMAND HANDLING

    public class GrabCommand : IEditorCommand
    {
        NavManager nav;
        EditorShipPart part;
        Vector2Int originCell;
        Vector2Int grabbedFromCell;

        public GrabCommand(NavManager nav, EditorShipPart part)
        {
            this.nav = nav;
            this.part = part;
            originCell = part.position;
            grabbedFromCell = nav.currentGridCell;
        }

        public void Execute()
        {
            part = nav.buildArea.GrabPart(nav.currentGridCell);

            nav.visualizer.MatchRectScale(part.rect);

            if (UIManager.Smoothing)
                nav.StartCoroutine(nav.GrabWithLerp(part, false));
            else
                nav.GrabImmediate(part, false);
        }

        public void Undo()
        {
            if (part)
            {
                nav.buildArea.PlacePart(originCell, part);
                part.OnPlaced(originCell, nav.buildArea);
            }
            else
            {
                nav.buildArea.PlacePart(originCell, nav.heldPart);
                nav.heldPart.OnPlaced(originCell, nav.buildArea);
            }
            nav.heldPart = null;
            nav.visualizer.ResetScale();

            nav.NavToCell(grabbedFromCell);
        }

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class PlaceCommand : IEditorCommand
    {
        NavManager nav;
        Vector2Int cell;
        EditorShipPart part;
        Vector2Int cellPlacedAt;

        public PlaceCommand(NavManager nav, Vector2Int cell)
        {
            this.nav = nav;
            this.cell = cell;
        }

        public void Execute()
        {
            part = nav.heldPart;
            nav.buildArea.PlacePart(cell, part);
            part.OnPlaced(cell, nav.buildArea);
            cellPlacedAt = part.cellPlacedAt;
            nav.heldPart = null;
            nav.visualizer.ResetScale();
            nav.NavToCell(part.position);
        }

        public void Undo()
        {
            if (part == null)
            {
                part = nav.buildArea.GrabPart(cellPlacedAt);
            }

            if (part)
            {
                nav.buildArea.GrabPart(part.cellPlacedAt);

                nav.visualizer.MatchRectScale(part.rect);

                if (UIManager.Smoothing)
                    nav.StartCoroutine(nav.GrabWithLerp(part, false));
                else
                    nav.GrabImmediate(part, false);
            }
        }

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class NavigateCommand : IEditorCommand
    {
        Vector2 totalInput;
        NavManager nav;

        public NavigateCommand(NavManager nav, Vector2 input)
        {
            this.nav = nav;
            this.totalInput = input;
        }

        public void Execute()
        {
            nav.TriggerNav(totalInput);
        }

        public void Undo()
        {
            nav.TriggerNav(-totalInput);
        }

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next)
        {
            if (next is not NavigateCommand other)
                return false;

            totalInput += other.totalInput;

            nav.TriggerNav(other.totalInput);

            return true;
        }
    }

    public class RotateCommand : IEditorCommand
    {
        float angle;
        NavManager nav;

        public RotateCommand(NavManager nav, float angle)
        {
            this.nav = nav;
            this.angle = angle;
        }

        public void Execute()
        {
            nav.RotatePart(angle);
        }

        public void Undo()
        {
            nav.RotatePart(-angle);
        }

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class FlipCommand : IEditorCommand
    {
        FlipAxis axis;
        NavManager nav;

        public FlipCommand(NavManager nav, FlipAxis axis)
        {
            this.nav = nav;
        }

        public void Execute()
        {
            nav.FlipPart(axis);
        }

        public void Undo()
        {
            nav.FlipPart(axis);
        }

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class ExitGridModeCommand : IEditorCommand
    {
        private NavManager nav;

        private ShipPartData partData;
        bool xFlipped;
        bool yFlipped;
        float rotation;

        public ExitGridModeCommand(NavManager nav)
        {
            this.nav = nav;

            if (nav.heldPart != null)
            {
                partData = nav.heldPart.partData;
                xFlipped = nav.heldPart.xFlipped;
                yFlipped = nav.heldPart.yFlipped;
                rotation = nav.heldPart.Rotation;
            }
        }

        public void Execute()
        {
            if (partData != null)
                nav.partOrganizer.AddPart(partData);

            if (nav.heldPart != null)
            {
                Destroy(nav.heldPart.gameObject);
                nav.heldPart = null;
            }

            nav.SwitchToItemMode();
        }

        public void Undo()
        {
            if (partData != null)
            {
                bool success = nav.partOrganizer.TryTakePart(partData, out EditorShipPart part);

                if (success)
                {
                    nav.partOrganizer.SetPartToDefaultStart(part);
                    nav.GrabImmediate(part, true);
                }
            }

            nav.SwitchToGridMode();

            if (nav.heldPart != null) nav.RestorePartTransformations(rotation, xFlipped, yFlipped);
        }

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class EnterGridModeCommand : IEditorCommand
    {
        private NavManager nav;

        public EnterGridModeCommand(NavManager nav)
        {
            this.nav = nav;
        }

        public void Execute() => nav.SwitchToGridMode();

        public void Undo() => nav.SwitchToItemMode();

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class InventoryGrabCommand : IEditorCommand
    {
        private ShipPartData partData;
        private NavManager nav;

        public InventoryGrabCommand(NavManager nav, ShipPartData data)
        {
            this.nav = nav;
            this.partData = data;
        }

        public void Execute()
        {
            bool success = nav.partOrganizer.TryTakePart(partData, out EditorShipPart newPart);

            if(UIManager.Smoothing)
                nav.GrabFrameLate(newPart, true);
            else
                nav.GrabImmediate(newPart, true);

            nav.SwitchToGridMode();
        }

        public void Undo()
        {
            nav.partOrganizer.AddPart(partData);

            if (nav.heldPart != null)
            {
                Destroy(nav.heldPart.gameObject);
                nav.heldPart = null;
            }

            nav.SwitchToItemMode();
        }

        public void Redo()
        {
            bool success = nav.partOrganizer.TryTakePart(partData, out EditorShipPart newPart);

            if (!success)
            {
                Debug.LogWarning("Redo failed: part not available.");
                return;
            }

            nav.partOrganizer.SetPartToDefaultStart(newPart);

            if (UIManager.Smoothing)
            {
                nav.StartCoroutine(nav.GrabWithLerp(newPart, true));
            }
            else
            {
                nav.GrabImmediate(newPart, true);
                nav.SwitchToGridMode();
            }

        }

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class DeleteCommand : IEditorCommand
    {
        NavManager nav;
        Vector2Int startCell;
        Vector2Int partPosition;
        ShipPartData partData;
        bool wasPlaced;
        bool xFlipped;
        bool yFlipped;
        float rotation;

        public DeleteCommand(NavManager nav, Vector2Int startCell)
        {
            this.nav = nav;
            this.startCell = startCell;
        }

        public void Execute()
        {
            if (nav.heldPart != null)
            {
                wasPlaced = false;
                nav.partOrganizer.AddPart(nav.heldPart.partData);
                partData = nav.heldPart.partData;

                xFlipped = nav.heldPart.xFlipped;
                yFlipped = nav.heldPart.yFlipped;
                rotation = nav.heldPart.Rotation;

                Destroy(nav.heldPart.gameObject);
            }
            else
            {
                wasPlaced = true;
                EditorShipPart part = nav.buildArea.GrabPart(nav.currentGridCell);
                partData = part.partData;
                partPosition = part.position;
                nav.partOrganizer.AddPart(part.partData);
                nav.GrabImmediate(part, false, false);

                xFlipped = nav.heldPart.xFlipped;
                yFlipped = nav.heldPart.yFlipped;
                rotation = nav.heldPart.Rotation;

                Destroy(part.gameObject);
            }

            nav.heldPart = null;
            nav.NavToCell(startCell);
        }

        public void Undo()
        {
            if (partData != null)
            {
                nav.midUndoDelete = true;
                if(wasPlaced) nav.visualizer.HighlightCellImmediate(partPosition, true);
                else nav.visualizer.HighlightCellImmediate(startCell, true);

                nav.StartCoroutine(nav.UndoDeleteRoutine(wasPlaced, partData, partPosition, startCell, rotation, xFlipped, yFlipped));
            }
        }

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next) => false;

    }

    public class ResetCommand : IEditorCommand
    {
        NavManager nav;
        Vector2Int prevCell;

        public ResetCommand(NavManager nav) 
        {
            this.nav = nav;
            prevCell = nav.currentGridCell;
        }

        public void Execute()
        {
            nav.InitNavMode(true);
        }

        public void Undo()
        {
            nav.NavToCell(prevCell);
        }

        public void Redo() => Execute();

        public bool TryMerge(IEditorCommand next) => false;

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
                    StartCoroutine(TryPlaceWithSync());
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
            InitNavMode(true);
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