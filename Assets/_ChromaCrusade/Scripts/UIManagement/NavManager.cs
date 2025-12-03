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
    public InputActionReference navigateAction;
    public InputActionReference submitAction;
    public InputActionReference cancelAction;

    [Header("Settings")]
    public float inputRepeatDelay = 0.35f;
    public float inputRepeatRate = 0.1f;

    public NavItem hoveredItem;

    private Vector2 navInput;
    private float nextRepeatTime;

    public struct DisableNavigationEvent { }
    public struct EnableNavigationEvent { }

    private void OnEnable()
    {
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();

        SubToInputs();

        EventBus.Subscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Subscribe<EnableNavigationEvent>(OnEnableNavigation);
    }


    private void OnDisable()
    {
        UnsubFromInputs();

        visualizer.gameObject.SetActive(false);

        EventBus.Unsubscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Unsubscribe<EnableNavigationEvent>(OnEnableNavigation);
    }

    private void Start()
    {
        visualizer.gameObject.SetActive(true);


        if (mode == NavMode.Item)
        {
            hoveredItem = hoveredItem ?? GetComponentInChildren<NavItem>();
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

    private void Update()
    {
        if (navInput == Vector2.zero)
            return;

        if (Time.time >= nextRepeatTime)
        {
            TriggerNav(navInput);
            nextRepeatTime = Time.time + inputRepeatRate;
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

    #region Input Actions

    private void OnNavigatePerformed(InputAction.CallbackContext ctx)
    {
        navInput = ctx.ReadValue<Vector2>();
        if(navInput == Vector2.zero) return;
        //Debug.Log("perf " + navInput.ToString());

        TriggerNav(navInput);

        nextRepeatTime = Time.time + inputRepeatDelay;
    }

    private void OnNavigateCanceled(InputAction.CallbackContext ctx)
    {
        navInput = Vector2.zero;
    }

    private void OnSubmitPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        hoveredItem?.OnSelected();
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        EventSystem.current.SetSelectedGameObject(null);
    }

    #endregion

    #region Input Management

    private void OnDisableNavigation(DisableNavigationEvent e)
    {
        UnsubFromMainInputs();
    }

    private void OnEnableNavigation(EnableNavigationEvent e)
    {
        SubToMainInputs();
    }

    private void SubToInputs()
    {
        SubToMainInputs();
        cancelAction.action.performed += OnCancelPerformed;
    }

    private void UnsubFromInputs()
    {
        UnsubFromMainInputs();
        cancelAction.action.performed -= OnCancelPerformed;
    }

    private void UnsubFromMainInputs()
    {
        navigateAction.action.performed -= OnNavigatePerformed;
        navigateAction.action.canceled -= OnNavigateCanceled;
        submitAction.action.performed -= OnSubmitPerformed;
    }

    private void SubToMainInputs()
    {
        navigateAction.action.performed += OnNavigatePerformed;
        navigateAction.action.canceled += OnNavigateCanceled;
        submitAction.action.performed += OnSubmitPerformed;
    }

    #endregion

}
