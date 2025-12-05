using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class NavManager : MonoBehaviour
{
    [Header("Mode")]
    public NavMode mode = NavMode.Item;
    public enum NavMode { Item, Grid };

    [Header("Grid Navigation")]
    [SerializeField] private Vector3Int currentGridCell = new Vector3Int(0,0,0);
    [SerializeField] private RectTransform centerGridCell;
    [SerializeField] private int zoomLevel = 3;
    [SerializeField] private Vector2 zoomRange = new Vector2(1, 10);
    public int ZoomLevel
    {
        get => zoomLevel;
        set => zoomLevel = (int)Mathf.Clamp(value, zoomRange.x, zoomRange.y);
    }

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


    public struct DisableNavigationEvent { }
    public struct EnableNavigationEvent { }
    public struct EnterInputFieldEvent { }

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

    private void Start()
    {
        visualizer.gameObject.SetActive(true);
        visualizer.centerGridCell = centerGridCell;

        InitNav();
    }

    private void Update()
    {
        ProcessNavInput();
    }

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

    private Vector2 prevStableInput = Vector2.zero;
    private Vector2 pendingCardinal = Vector2.zero;
    private float pendingTime = 0f;
    private int prevStableDiagonalHits = 0; // fix for direct transition from diag to card
    private int hitThreshold = 5; // could make this frame-dependent like pendingWindow
    private Vector2 FilterDiagonalTransitions(Vector2 raw)
    {
        bool rawIsNeutral = IsNeutral(raw);
        bool rawIsCardinal = IsCardinal(raw);
        bool rawIsDiagonal = IsDiagonal(raw);

        if (rawIsDiagonal)
        {
            pendingCardinal = Vector2.zero;
            return prevStableInput = raw; // always accept diag
        }

        if (rawIsNeutral)
        {
            pendingCardinal = Vector2.zero;
            return prevStableInput = Vector2.zero; // always accept neutral
        }

        if (rawIsCardinal)
        {
            float pendingWindow = Mathf.Clamp(Time.deltaTime * 3f, 0.02f, 0.04f);

            if (IsDiagonal(prevStableInput))
            {
                prevStableDiagonalHits++;
                if (prevStableDiagonalHits > hitThreshold) return prevStableInput = raw;
                return prevStableInput; // deny first cardinal
            }
            prevStableDiagonalHits = 0;

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
        Vector3Int offset = Vector3Int.zero;

        if (dir.y > 0.5f) offset.y += 1;
        if (dir.y < -0.5f) offset.y -= 1;
        if (dir.x < -0.5f) offset.x -= 1;
        if (dir.x > 0.5f) offset.x += 1;

        if (offset == Vector3Int.zero)
            return;

        Vector3Int newCell = currentGridCell + offset;

        currentGridCell = newCell;

        visualizer.OnHighlightGridCell(currentGridCell);
    }

    private void NavToItem(NavItem item)
    {
        if(item == null) return;
        hoveredItem = item;
        hoveredItem.OnHighlighted();
        visualizer.OnHighlightItem(hoveredItem);
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
        EnterNavMode(false);
    }

    public void SwitchToItemMode() => SwitchNavMode(NavMode.Item);

    public void SwitchToGridMode() => SwitchNavMode(NavMode.Grid);

    private void InitItemNav()
    {
        NavToItem(initialHoveredItem ?? GetComponentInChildren<NavItem>());
    }
    
    private void ReturnToGridNav()
    {
        hoveredItem = null;
        visualizer.OnHighlightGridCell(currentGridCell);
    }

    private void EnterNavMode(bool resetGrid)
    {
        switch (mode)
        {
            case NavMode.Item:
                InitItemNav();
                break;

            case NavMode.Grid:
                if (resetGrid) currentGridCell = Vector3Int.zero;
                ReturnToGridNav();
                break;
        }
    }

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

    #region Input Actions

    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        ZoomLevel += Mathf.RoundToInt(input);
    }

    private void OnSubmitPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        hoveredItem?.OnSelected();
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
        EnterNavMode(true);
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
