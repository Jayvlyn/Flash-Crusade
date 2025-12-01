using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class NavManager : MonoBehaviour
{
    [Header("Visualization")]
    public NavVisualizer visualizer;

    [Header("Navigation Input")]
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

        SubscribeToInputs();

        EventBus.Subscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Subscribe<EnableNavigationEvent>(OnEnableNavigation);
    }


    private void OnDisable()
    {
        UnsubscribeFromInputs();

        visualizer.gameObject.SetActive(false);

        EventBus.Unsubscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Unsubscribe<EnableNavigationEvent>(OnEnableNavigation);
    }

    private void Start()
    {
        visualizer.gameObject.SetActive(true);
        hoveredItem = hoveredItem ?? GetComponentInChildren<NavItem>();
        if (hoveredItem)
            visualizer.OnHighlightNew(hoveredItem);
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

    private void OnNavigatePerformed(InputAction.CallbackContext ctx)
    {
        navInput = ctx.ReadValue<Vector2>();

        TriggerNav(navInput);

        nextRepeatTime = Time.time + inputRepeatDelay;
    }

    private void OnNavigateCanceled(InputAction.CallbackContext ctx)
    {
        navInput = Vector2.zero;
    }

    private void OnSubmitPerformed(InputAction.CallbackContext ctx)
    {
        hoveredItem?.OnSelected();
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("firing");
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void TriggerNav(Vector2 dir)
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

        hoveredItem = next;
        hoveredItem.OnHighlighted();
        visualizer.OnHighlightNew(hoveredItem);
    }

    private void OnDisableNavigation(DisableNavigationEvent e)
    {
        LimitInputs();
    }

    private void OnEnableNavigation(EnableNavigationEvent e)
    {
        UnlimitInputs();
    }

    private void SubscribeToInputs()
    {
        navigateAction.action.performed += OnNavigatePerformed;
        navigateAction.action.canceled += OnNavigateCanceled;
        submitAction.action.performed += OnSubmitPerformed;
        cancelAction.action.performed += OnCancelPerformed;
    }

    private void UnsubscribeFromInputs()
    {
        navigateAction.action.performed -= OnNavigatePerformed;
        navigateAction.action.canceled -= OnNavigateCanceled;
        submitAction.action.performed -= OnSubmitPerformed;
        cancelAction.action.performed -= OnCancelPerformed;
    }

    private void LimitInputs()
    {
        navigateAction.action.performed -= OnNavigatePerformed;
        navigateAction.action.canceled -= OnNavigateCanceled;
        submitAction.action.performed -= OnSubmitPerformed;
    }

    private void UnlimitInputs()
    {
        navigateAction.action.performed += OnNavigatePerformed;
        navigateAction.action.canceled += OnNavigateCanceled;
        submitAction.action.performed += OnSubmitPerformed;
    }

}
