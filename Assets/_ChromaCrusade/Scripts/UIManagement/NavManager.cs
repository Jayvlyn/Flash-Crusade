using System.Collections;
using System.Collections.Generic;
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

    [Header("Visualization")]
    [SerializeField] private NavVisualizer visualizer;

    [Header("Input")]
    [SerializeField] private InputActionReference navigateAction;
    [SerializeField] private InputActionReference submitAction;
    [SerializeField] private InputActionReference cancelAction;
    [SerializeField] private InputActionReference modeAction;
    [SerializeField] private InputActionReference resetAction;
    [SerializeField] private InputActionReference zoomAction;
    private Vector2 lastMoveInput = Vector2.zero;
    private bool inInputField = false;
    private bool allowMovement;

    [Header("Settings")]
    [SerializeField] private float inputRepeatDelay = 0.35f;
    [SerializeField] private float inputRepeatRate = 0.1f;

    [Header("Other Refs")]
    private NavItem hoveredItem;
    [SerializeField] private NavItem initialHoveredItem;
    [SerializeField] private NavItem exitItem;
    [SerializeField] private EditorBuildArea buildArea;
    private EditorShipPart heldPart;

    [Header("TESTING")]
    public EditorShipPart testPart;

    #endregion

    #region Events
    public struct DisableNavigationEvent { }
    public struct EnableNavigationEvent { }
    public struct EnterInputFieldEvent { }
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
        //TESTING
        buildArea.PlacePart(new Vector2Int(6, 6), testPart);
        //----

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
        if (!allowMovement) return;

        Vector2 raw = navigateAction.action.ReadValue<Vector2>();
        Vector2 input = FilterDiagonalTransitions(raw);

        if (input == Vector2.zero)
        {
            lastMoveInput = Vector2.zero;
            nextRepeatTime = 0f;
            return;
        }

        bool newInput = input != lastMoveInput;

        if (newInput || Time.time >= nextRepeatTime)
        {
            TriggerNav(input);
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
        Vector2Int offset = Vector2Int.zero;

        if (dir.y > 0.5f) offset.y += 1;
        if (dir.y < -0.5f) offset.y -= 1;
        if (dir.x < -0.5f) offset.x -= 1;
        if (dir.x > 0.5f) offset.x += 1;

        if (offset == Vector2Int.zero)
            return;

        Vector2Int newCell = currentGridCell + offset;

        currentGridCell = newCell;

        visualizer.OnHighlightGridCell(currentGridCell, heldPart != null);
    }

    private void NavToItem(NavItem item)
    {
        if(item == null) return;
        hoveredItem = item;
        hoveredItem.OnHighlighted();
        visualizer.OnHighlightItem(hoveredItem);
    }

    private void NavToCell(Vector2Int cell)
    {
        currentGridCell = cell;
        visualizer.OnHighlightGridCell(currentGridCell);
    }

    public void ToggleNavMode()
    {
        if (mode == NavMode.Item)      SwitchToGridMode();
        else if (mode == NavMode.Grid) SwitchToItemMode();
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
            SwitchToItemMode();
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
        visualizer.OnHighlightGridCell(currentGridCell);
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
    private void TryGrabPart()
    {
        Debug.Log("Trying to grab part at " + currentGridCell.ToString());
        EditorShipPart part = buildArea.GrabPart(currentGridCell);
        if (part)
        {
            visualizer.UpdateWithRectImmediate(part.rect);
            part.OnGrabbed(visualizer.rect);
            currentGridCell = part.position;
            heldPart = part;
        }
    }

    private void TryPlacePart()
    {
        Debug.Log("Trying to place part at " + currentGridCell.ToString());
        if (buildArea.PlacePart(currentGridCell, heldPart))
        {
            heldPart.rect.parent = buildArea.rect;
            heldPart.OnPlaced(currentGridCell);
            heldPart = null;
            visualizer.UpdateGridCellImmediate(currentGridCell);
        }
    }


    #endregion

    #region Input Actions

    private Coroutine zoomRoutine;
    private Vector3 targetZoomScale;
    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        ZoomLevel -= Mathf.RoundToInt(input);
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
            visualizer.UpdateGridCellImmediate(currentGridCell);
        }
    }
    private IEnumerator LerpZoom(Vector3 target, float duration = 0.15f)
    {
        float t = 0f;
        Vector3 start = buildArea.transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            buildArea.transform.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            visualizer.UpdateGridCellImmediate(currentGridCell);

            yield return null;
        }
        buildArea.transform.localScale = target;
    }

    private void OnSubmitPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        // submit is disabled in input field, no need to consider that case
        Debug.Log("submit performed: " + mode.ToString());

        if(mode == NavMode.Item)
        {
            hoveredItem?.OnSelected();
        }
        else if(mode == NavMode.Grid)
        {
            if(heldPart != null)
            {
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
    }

    private void EnableInputs()
    {
        EnableMainInputs();
        cancelAction.action.performed += OnCancelPerformed;
        modeAction.action.performed += OnModePerformed;
        resetAction.action.performed += OnResetPerformed;
        zoomAction.action.performed += OnZoomPerformed;
    }

    private void DisableInputs()
    {
        DisableMainInputs();
        cancelAction.action.performed -= OnCancelPerformed;
        modeAction.action.performed -= OnModePerformed;
        resetAction.action.performed -= OnResetPerformed;
        zoomAction.action.performed -= OnZoomPerformed;
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
}