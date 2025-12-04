using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class NavManager : MonoBehaviour
{
    [Header("Mode")]
    public NavMode mode = NavMode.Item;
    public enum NavMode { Item, Grid };

    [Header("Grid Navigation")]
    public Grid grid;
    public Transform gridRoot;
    public Vector3Int currentGridCell = new Vector3Int(0,0,0);

    [Header("Visualization")]
    public NavVisualizer visualizer;

    [Header("Input")]
    private bool allowMovement;
    public InputActionReference navigateAction;
    public InputActionReference submitAction;
    public InputActionReference cancelAction;
    public InputActionReference modeAction;
    public InputActionReference resetAction;

    [Header("Settings")]
    public float inputRepeatDelay = 0.35f;
    public float inputRepeatRate = 0.1f;

    private NavItem hoveredItem;
    public NavItem initialHoveredItem;
    public NavItem exitItem;

    private float nextRepeatTime;

    private bool inInputField = false;

    private Vector2 lastMoveInput = Vector2.zero;

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
        Application.targetFrameRate = 60;

        visualizer.gameObject.SetActive(true);

        InitNav();
    }

    private void Update()
    {
        ProcessNavInput();
    }

    private void ProcessNavInput()
    {
        if (!allowMovement) return;

        Vector2 raw = navigateAction.action.ReadValue<Vector2>();
        Debug.Log(raw);
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
    //private const float pendingWindow = 0.05f;

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
            float pendingWindow = Mathf.Clamp(Time.deltaTime * 3f, 0.02f, 0.12f);

            if (IsDiagonal(prevStableInput))
                return prevStableInput; // deny first cardinal after diagonal

            // Special case: cardinal sandwiched between neutrals
            if (IsNeutral(prevStableInput))
            {
                // Next frame, if we return to neutral, this should still be accepted
                // So immediately accept this cardinal press
                pendingCardinal = Vector2.zero;
                return prevStableInput = raw;
            }

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

        visualizer.OnHighlightGridCell(grid, currentGridCell);
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
        OnEnterNewNavMode();
    }
    public void SwitchToItemMode() => SwitchNavMode(NavMode.Item);
    public void SwitchToGridMode() => SwitchNavMode(NavMode.Grid);

    private void InitItemNav()
    {
        NavToItem(initialHoveredItem ?? GetComponentInChildren<NavItem>());
    }

    private void InitGridNav()
    {
        currentGridCell = new Vector3Int(0, 0, 0);
        ReturnToGridNav();
    }

    private void ReturnToGridNav()
    {
        hoveredItem = null;
        visualizer.OnHighlightGridCell(grid, currentGridCell);
    }

    private void OnEnterNewNavMode()
    {
        if (mode == NavMode.Item)
            InitItemNav();
        else if (mode == NavMode.Grid)
            ReturnToGridNav();
    }

    private void InitNav()
    {
        if (mode == NavMode.Item)
            InitItemNav();
        else if (mode == NavMode.Grid)
            InitGridNav();
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

    // For our navigation composite we dont use input action anymore, because simulatious directional
    // movement was impossible, now we poll the value of the navigation action

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
        InitNav();
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
    }

    private void EnableInputs()
    {
        EnableMainInputs();
        cancelAction.action.performed += OnCancelPerformed;
        modeAction.action.performed += OnModePerformed;
        resetAction.action.performed += OnResetPerformed;
    }

    private void DisableInputs()
    {
        DisableMainInputs();
        cancelAction.action.performed -= OnCancelPerformed;
        modeAction.action.performed -= OnModePerformed;
        resetAction.action.performed -= OnResetPerformed;
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
