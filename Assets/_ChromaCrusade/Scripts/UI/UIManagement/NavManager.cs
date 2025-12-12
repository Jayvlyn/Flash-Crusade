using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class NavManager : MonoBehaviour
{
    #region Variables

    [Header("Mode")]
    public NavMode mode = NavMode.Item;
    public enum NavMode { Item, Grid };

    [Header("Grid Navigation")]
    [SerializeField] private Vector2Int currentGridCell = new Vector2Int(0,0);
    [SerializeField] private RectTransform centerGridCell;
    [SerializeField] private int zoomLevel = 3;
    [SerializeField] private Vector2 zoomRange = new Vector2(1, 10);
    public int ZoomLevel
    {
        get => zoomLevel;
        set => zoomLevel = (int)Mathf.Clamp(value, zoomRange.x, zoomRange.y);
    }
    private Dictionary<int,float> zoomScales = new Dictionary<int,float>();
    private bool Expanded => heldPart != null;

    [Header("Visualization")]
    [SerializeField] private NavVisualizer visualizer;

    [Header("Input")]
    [SerializeField] private InputActionReference navigateAction;
    [SerializeField] private InputActionReference submitAction;
    [SerializeField] private InputActionReference cancelAction;
    [SerializeField] private InputActionReference modeAction;
    [SerializeField] private InputActionReference resetAction;
    [SerializeField] private InputActionReference zoomAction;
    [SerializeField] private InputActionReference undoAction;
    [SerializeField] private InputActionReference redoAction;
    [SerializeField] private InputActionReference rotateAction;
    [SerializeField] private InputActionReference flipAction;
    [SerializeField] private InputActionReference modifyAction;
    private Vector2 lastMoveInput;
    private bool inInputField;
    private bool allowMovement;
    private bool modifyHeld;

    [Header("Settings")]
    [SerializeField] private float inputRepeatDelay = 0.35f;
    [SerializeField] private float inputRepeatRate = 0.1f;

    [Header("Other Refs")]
    private NavItem hoveredItem;
    [SerializeField] private NavItem initialHoveredItem;
    [SerializeField] private NavItem exitItem;
    [SerializeField] private EditorBuildArea buildArea;
    private EditorShipPart heldPart;
    [HideInInspector] public PartOrganizer partOrganizer;

    #endregion

    #region Events
    public struct DisableNavigationEvent { }
    public struct EnableNavigationEvent { }
    public struct EnterInputFieldEvent { }
    public struct InventoryPartGrabbedEvent { EditorShipPart part; }
    #endregion

    #region Lifecycle

    private void OnEnable()
    {
        EnableActions();
        EnableInputs();

        EventBus.Subscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Subscribe<EnableNavigationEvent>(OnEnableNavigation);
        EventBus.Subscribe<EnterInputFieldEvent>(OnEnterInputField);
    }

    private void OnDisable()
    {
        DisableInputs();

        visualizer.gameObject.SetActive(false);

        EventBus.Unsubscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Unsubscribe<EnableNavigationEvent>(OnEnableNavigation);
        EventBus.Unsubscribe<EnterInputFieldEvent>(OnEnterInputField);
    }

    private void Awake()
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

    private void Start()
    {
        visualizer.gameObject.SetActive(true);
        visualizer.centerGridCell = centerGridCell;

        if (buildArea == null) buildArea = FindFirstObjectByType<EditorBuildArea>();
        if (visualizer == null) visualizer = FindFirstObjectByType<NavVisualizer>();

        InitNavMode(true);
    }

    private void Update()
    {
        ProcessNavInput();
    }

    #endregion

    #region Navigation

    private float nextRepeatTime;
    private void ProcessNavInput()
    {
        if (!allowMovement || midGrab || midZoom) return;

        Vector2 raw = navigateAction.action.ReadValue<Vector2>();
        Vector2 input = FilterDiagonalTransitions(raw);

        if (input == Vector2.zero)
        {
            lastMoveInput = Vector2.zero;
            nextRepeatTime = 0f;
            return;
        }

        input.x = Mathf.RoundToInt(input.x);
        input.y = Mathf.RoundToInt(input.y);
        if (modifyHeld)
        {
            input.x *= 3;
            input.y *= 3;
        }

        bool newInput = input != lastMoveInput;

        if (newInput || Time.time >= nextRepeatTime)
        {
            if (mode == NavMode.Grid) CommandHistory.Execute(new NavigateCommand(this, input));
            else TriggerNav(input);
            nextRepeatTime = Time.time + (newInput ? inputRepeatDelay : inputRepeatRate);
            lastMoveInput = input;
        }
    }

    private void TriggerNav(Vector2 dir)
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

    private void TriggerNavItem(Vector2 dir)
    {
        if (hoveredItem == null)
            return;

        NavItem next = null;

        if (dir.y > 0.5f) next = hoveredItem.navUp;
        else if (dir.y < -0.5f) next = hoveredItem.navDown;
        else if (dir.x < -0.5f) next = hoveredItem.navLeft;
        else if (dir.x > 0.5f) next = hoveredItem.navRight;

        if (next == null)
            return;

        NavToItem(next);
    }

    private void TriggerNavGrid(Vector2 dir)
    {
        Vector2Int offset = new Vector2Int((int)dir.x, (int)dir.y);

        if (offset == Vector2Int.zero)
            return;
        
        Vector2Int newCell = currentGridCell + offset;

        currentGridCell = newCell;

        visualizer.HighlightCell(currentGridCell, Expanded);
    }

    private void NavToItem(NavItem item)
    {
        if(item == null) return;
        hoveredItem = item;
        hoveredItem.OnHighlighted();
        visualizer.HighlightItem(hoveredItem);
    }

    private void NavToCell(Vector2Int cell)
    {
        currentGridCell = cell;
        visualizer.HighlightCell(currentGridCell, Expanded);
    }

    public void ToggleNavMode()
    {
        if (mode == NavMode.Item) SwitchToGridMode();
        else if (mode == NavMode.Grid) CommandHistory.Execute(new ExitBuildModeCommand(this));
    }

    public void SwitchNavMode(NavMode newMode)
    {
        if (mode == newMode) return;
        mode = newMode;
        InitNavMode(false);
    }

    public void SwitchToItemMode() => SwitchNavMode(NavMode.Item);

    public void SwitchToGridMode() => SwitchNavMode(NavMode.Grid);

    private void GoBack()
    {
        if (inInputField)
        {
            inInputField = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (mode == NavMode.Item)
        {
            if (hoveredItem == exitItem) exitItem.OnSelected();
            else NavToItem(exitItem);
        }
        else if (mode == NavMode.Grid)
        {
            CommandHistory.Execute(new ExitBuildModeCommand(this));
        }
    }

    #endregion

    #region Initialization

    private void InitItemNav()
    {
        NavToItem(initialHoveredItem ?? GetComponentInChildren<NavItem>());
    }
    
    private void InitGridNav()
    {
        hoveredItem = null;
        visualizer.HighlightCell(currentGridCell, Expanded);
    }

    private void InitNavMode(bool resetGrid)
    {
        switch (mode)
        {
            case NavMode.Item:
                InitItemNav();
                break;

            case NavMode.Grid:
                if (resetGrid) currentGridCell = Vector2Int.zero;
                InitGridNav();
                break;
        }
    }

    #endregion

    #region Part Manipulation

    #region Grab
    private void TryGrabPart()
    {
        //Debug.Log("Trying to grab part at " + currentGridCell.ToString());

        EditorShipPart part = buildArea.GetPartAtCell(currentGridCell);
        if (part)
        {
            CommandHistory.Execute(new GrabCommand(this, part));
        }
    }

    private void GrabFrameLate(EditorShipPart part, bool fromInv)
    {
        heldPart = part;
        StartCoroutine(FrameLateRoutine(part, fromInv));
    }

    private IEnumerator FrameLateRoutine(EditorShipPart part, bool fromInv)
    {
        yield return null;
        GrabImmediate(part, fromInv);
    }

    private void GrabImmediate(EditorShipPart part, bool fromInv)
    {
        visualizer.UpdateWithRectImmediate(part.rect);
        part.OnGrabbed(visualizer.rect);
        if(!fromInv) currentGridCell = part.position;
        heldPart = part;
    }

    private bool midGrab = false;
    private IEnumerator GrabWithLerp(EditorShipPart part, bool fromInv)
    {
        midGrab = true;
        yield return visualizer.LerpWithRect(part.rect); // waits until done

        part.OnGrabbed(visualizer.rect);
        if (mode == NavMode.Grid) currentGridCell = part.position;
        heldPart = part;
        midGrab = false;
    }

    public void OnInventoryPartGrabbed(ShipPartData part)
    {
        CommandHistory.Execute(new InventoryGrabCommand(this, part));
    }

    #endregion

    #region Place
    private void TryPlacePart()
    {
        if(buildArea.CanPlacePart(currentGridCell, heldPart))
        {
            CommandHistory.Execute(new PlaceCommand(this, currentGridCell));
        }
    }

    private bool placeQueued;
    private IEnumerator TryPlaceWithSync()
    {
        if (placeQueued) yield break; // prevents spam stacking
        placeQueued = true;

        if (UIManager.Smoothing && visualizer.IsLerping)
            yield return visualizer.WaitUntilDone();

        TryPlacePart(); // safe now
        placeQueued = false;
    }

    #endregion

    #region Flip & Rotate

    public void RotatePart(float angle)
    {
        heldPart.Rotate(angle);
        visualizer.Rotate(angle);
    }

    public void FlipPart(float input)
    { // input: 1 = vert flip    -1 = hori flip
        bool horizontal = input == -1;
        if (heldPart.Rotation == 90 || heldPart.Rotation == 270) horizontal = !horizontal;
        heldPart.Flip(horizontal);
        visualizer.Flip(horizontal);
    }

    #endregion

    #endregion

    #region Zoom

    private Coroutine zoomRoutine;
    private Vector3 targetZoomScale;
    private void OnNewZoomLevel()
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
    private IEnumerator LerpZoom(Vector3 target, float duration = 0.15f)
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

    #endregion

    #region Input Actions

    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        ZoomLevel -= Mathf.RoundToInt(input);
        OnNewZoomLevel();
    }

    private void OnSubmitPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        // submit is disabled when in input field, no need to consider that case

        if(mode == NavMode.Item)
        {
            hoveredItem?.OnSelected();
        }
        else if(mode == NavMode.Grid)
        {
            if (midGrab) return;
            if(heldPart != null)
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

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        GoBack();
    }

    private void OnModePerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        ToggleNavMode();
    }

    private void OnResetPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        InitNavMode(true);
    }

    private void OnUndoPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        //if (mode != NavMode.Grid) return;
        if (visualizer.IsRotateLerping) return;
        if (visualizer.IsFlipLerping) return;
        if (visualizer.IsLerping) return;

        CommandHistory.Undo();
    }

    private void OnRedoPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        //if (mode != NavMode.Grid) return;
        if (visualizer.IsRotateLerping) return;
        if (visualizer.IsFlipLerping) return;
        if (visualizer.IsLerping) return;

        CommandHistory.Redo();
    }

    private void OnRotatePerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        if (heldPart == null) return;
        if (mode != NavMode.Grid) return;
        if (visualizer.IsRotateLerping) return;

        float input = ctx.ReadValue<float>();
        input *= 90;
        if (modifyHeld) input *= 1.999f; // comes out to ~179.9 so that lerp happens in correct direction, will snap to int later
        CommandHistory.Execute(new RotateCommand(this, input));
    }

    private void OnFlipPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        if (heldPart == null) return;
        if (mode != NavMode.Grid) return;
        if (visualizer.IsFlipLerping) return;

        float input = ctx.ReadValue<float>();
        if (input == 0) return;
        CommandHistory.Execute(new FlipCommand(this, input));
    }

    private void OnModifyPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();

        if (input == 1) modifyHeld = true;
        else modifyHeld = false;
    }

    #endregion

    #region Input Management

    private void OnDisableNavigation(DisableNavigationEvent e)
    {
        DisableMainInputs();
    }

    private void OnEnableNavigation(EnableNavigationEvent e)
    {
        EnableMainInputs();
    }

    private void OnEnterInputField(EnterInputFieldEvent e)
    {
        inInputField = true;   
    }

    private void EnableActions()
    {
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();
        modeAction.action.Enable();
        resetAction.action.Enable();
        zoomAction.action.Enable();
        undoAction.action.Enable();
        redoAction.action.Enable();
        rotateAction.action.Enable();
        flipAction.action.Enable();
        modifyAction.action.Enable();
    }

    private void EnableInputs()
    {
        EnableMainInputs();
        cancelAction.action.performed += OnCancelPerformed;
        modeAction.action.performed += OnModePerformed;
        resetAction.action.performed += OnResetPerformed;
        zoomAction.action.performed += OnZoomPerformed;
        undoAction.action.performed += OnUndoPerformed;
        redoAction.action.performed += OnRedoPerformed;
        rotateAction.action.performed += OnRotatePerformed;
        flipAction.action.performed += OnFlipPerformed;
        modifyAction.action.performed += OnModifyPerformed;
    }

    private void DisableInputs()
    {
        DisableMainInputs();
        cancelAction.action.performed -= OnCancelPerformed;
        modeAction.action.performed -= OnModePerformed;
        resetAction.action.performed -= OnResetPerformed;
        zoomAction.action.performed -= OnZoomPerformed;
        undoAction.action.performed -= OnUndoPerformed;
        redoAction.action.performed -= OnRedoPerformed;
        rotateAction.action.performed -= OnRotatePerformed;
        flipAction.action.performed -= OnFlipPerformed;
        modifyAction.action.performed -= OnModifyPerformed;
    }

    private void EnableMainInputs()
    {
        allowMovement = true;
        submitAction.action.performed += OnSubmitPerformed;
    }

    private void DisableMainInputs()
    {
        allowMovement = false;
        submitAction.action.performed -= OnSubmitPerformed;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            modifyHeld = modifyAction.action.IsPressed();
        }
        else
        {
            modifyHeld = false;
        }
    }

    #endregion

    #region Helpers

    private Vector2 prevStableInput = Vector2.zero;
    private Vector2 pendingCardinal = Vector2.zero;
    private Vector2 pendingDiagonal = Vector2.zero;
    private float pendingTime;
    private int stuckHits;
    private int stuckHitThreshold = 5; // could make this frame-dependent like pendingWindow
    private float cardinalHoldTime;
    private float diagonalHoldTime;
    private Vector2 FilterDiagonalTransitions(Vector2 raw)
    {
        bool rawIsNeutral = IsNeutral(raw);
        bool rawIsCardinal = IsCardinal(raw);
        bool rawIsDiagonal = IsDiagonal(raw);
        float pendingWindow = Mathf.Clamp(Time.deltaTime * 3f, 0.02f, 0.04f);
        if (rawIsDiagonal)
        {
            diagonalHoldTime += Time.deltaTime;

            if (IsCardinal(prevStableInput))
            {
                if(cardinalHoldTime > 0.2f || diagonalHoldTime > 0.2f)
                {
                    cardinalHoldTime = 0;
                    diagonalHoldTime = 0;
                    return prevStableInput = raw; // accept diagonal after being stuck
                }
                return prevStableInput; // deny diagonal
            }

            if (pendingDiagonal == Vector2.zero)
            {
                pendingDiagonal = raw;
                pendingTime = Time.time;
                return prevStableInput; // new pending diag, use prev
            }

            if (Time.time - pendingTime < pendingWindow)
            {
                return prevStableInput; // diag still pending, keep using previous
            }

            pendingCardinal = Vector2.zero;
            return prevStableInput = raw; // accept diag after pending
        }

        if (rawIsNeutral)
        {
            cardinalHoldTime = 0;
            diagonalHoldTime = 0;
            pendingCardinal = Vector2.zero;
            return prevStableInput = Vector2.zero; // always accept neutral
        }

        if (rawIsCardinal)
        {
            cardinalHoldTime += Time.deltaTime;

            if (IsDiagonal(prevStableInput))
            {
                stuckHits++;
                if (stuckHits > stuckHitThreshold)
                {
                    stuckHits = 0;
                    return prevStableInput = raw; // accept cardinal after being stuck
                }
                return prevStableInput; // deny cardinal
            }
            stuckHits = 0;

            if (pendingCardinal == Vector2.zero)
            {
                pendingCardinal = raw;
                pendingTime = Time.time;
                return prevStableInput; // new pending cardinal, keep using previous
            }

            if (Time.time - pendingTime < pendingWindow)
            {
                return prevStableInput; // cardinal still pending, keep using previous
            }

            pendingCardinal = Vector2.zero;
            return prevStableInput = raw; // cardinal accepted after pending
        }

        return prevStableInput;
    }

    private bool IsDiagonal(Vector2 v)
    {
        return Mathf.Abs(v.x) > 0.1f && Mathf.Abs(v.y) > 0.1f;
    }

    private bool IsCardinal(Vector2 v)
    {
        return Mathf.Abs(v.x) > 0.1f ^ Mathf.Abs(v.y) > 0.1f;
    }

    private bool IsNeutral(Vector2 v)
    {
        return v.sqrMagnitude < 0.01f;
    }

    #endregion

    #region Commands

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
            nav.buildArea.GrabPart(nav.currentGridCell);
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
                nav.heldPart = null;
                nav.currentGridCell = grabbedFromCell;
                nav.visualizer.HighlightCell(grabbedFromCell);
            }
        }

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class PlaceCommand : IEditorCommand
    {
        NavManager nav;
        Vector2Int cell;
        EditorShipPart part;

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
            nav.heldPart = null;
            nav.visualizer.HighlightCell(part.position);
        }

        public void Undo()
        {
            if (part)
            {
                nav.buildArea.GrabPart(part.cellPlacedAt);
                if (UIManager.Smoothing)
                    nav.StartCoroutine(nav.GrabWithLerp(part, false));
                else
                    nav.GrabImmediate(part, false);
            }
        }

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

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class FlipCommand : IEditorCommand
    {
        float input;
        NavManager nav;

        public FlipCommand(NavManager nav, float input)
        {
            this.nav = nav;
            this.input = input;
        }

        public void Execute()
        {
            nav.FlipPart(input);
        }

        public void Undo()
        {
            nav.FlipPart(input);
        }

        public bool TryMerge(IEditorCommand next) => false;
    }

    public class ExitBuildModeCommand : IEditorCommand
    {
        private ShipPartData partData;
        private NavManager nav;

        public ExitBuildModeCommand(NavManager nav)
        {
            this.nav = nav;

            if (nav.heldPart != null)
                partData = nav.heldPart.partData;
        }

        public void Execute()
        {
            if (partData != null)
                nav.partOrganizer.AddPart(partData);

            if (nav.heldPart != null)
            {
                Object.Destroy(nav.heldPart.gameObject);
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
                    nav.GrabImmediate(part, true);
                }
            }


            nav.SwitchToGridMode();
        }

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
            // Always pull from inventory on execution
            bool success = nav.partOrganizer.TryTakePart(partData, out EditorShipPart newPart);

            if (!success)
            {
                Debug.LogWarning("Redo failed: part not available.");
                return;
            }

            //nav.GrabImmediate(newPart);
            nav.GrabFrameLate(newPart, true);
            nav.SwitchToGridMode();
        }

        public void Undo()
        {
            // return to inventory
            nav.partOrganizer.AddPart(partData);

            if (nav.heldPart != null)
            {
                Object.Destroy(nav.heldPart.gameObject);
                nav.heldPart = null;
            }

            nav.SwitchToItemMode();
        }

        public bool TryMerge(IEditorCommand next) => false;
    }


    #endregion
}