using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static NavManager;

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

    [Header("Settings")]
    public float inputRepeatDelay = 0.35f;
    public float inputRepeatRate = 0.1f;

    private NavItem hoveredItem;
    public NavItem initialHoveredItem;

    //private Vector2 navInput;
    private float nextRepeatTime;

    public struct DisableNavigationEvent { }
    public struct EnableNavigationEvent { }

    private void OnEnable()
    {
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();

        EnableInputs();

        EventBus.Subscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Subscribe<EnableNavigationEvent>(OnEnableNavigation);
    }


    private void OnDisable()
    {
        DisableInputs();

        visualizer.gameObject.SetActive(false);

        EventBus.Unsubscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Unsubscribe<EnableNavigationEvent>(OnEnableNavigation);
    }

    private void Start()
    {
        visualizer.gameObject.SetActive(true);

        OnEnterNewNavMode();
    }

    private Vector2 lastMoveInput = Vector2.zero;

    private void Update()
    {
        Vector2 input = navigateAction.action.ReadValue<Vector2>();

        if (input == Vector2.zero)
        {
            lastMoveInput = Vector2.zero;
            nextRepeatTime = 0f;
            return;
        }

        if (allowMovement && input != lastMoveInput || Time.time >= nextRepeatTime)
        {
            TriggerNav(input);
            nextRepeatTime = Time.time + (input != lastMoveInput ? inputRepeatDelay : inputRepeatRate);
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

    // Currently limitless nav, no bounds
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
        hoveredItem = item;
        hoveredItem.OnHighlighted();
        visualizer.OnHighlightItem(hoveredItem);
    }

    public void SwitchNavMode(NavMode newMode)
    {
        if (mode == newMode) return;
        mode = newMode;
        OnEnterNewNavMode();
    }
    public void SwitchToItemMode() => SwitchNavMode(NavMode.Item);
    public void SwitchToGridMode() => SwitchNavMode(NavMode.Grid);

    private void OnEnterNewNavMode()
    {
        if (mode == NavMode.Item)
        {
            //hoveredItem = hoveredItem ?? GetComponentInChildren<NavItem>();
            hoveredItem = initialHoveredItem ?? GetComponentInChildren<NavItem>();
            if (hoveredItem)
                visualizer.OnHighlightItem(hoveredItem);
        }
        else if (mode == NavMode.Grid)
        {
            hoveredItem = null;
            Vector3 worldPos = grid.CellToWorld(currentGridCell);
            visualizer.OnHighlightGridCell(grid, currentGridCell);
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
        EventSystem.current.SetSelectedGameObject(null);
        SwitchToItemMode();
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

    private void EnableInputs()
    {
        EnableMainInputs();
        cancelAction.action.performed += OnCancelPerformed;
    }

    private void DisableInputs()
    {
        DisableMainInputs();
        cancelAction.action.performed -= OnCancelPerformed;
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

}
