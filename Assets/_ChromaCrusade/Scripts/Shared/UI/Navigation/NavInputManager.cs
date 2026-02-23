using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class NavInputManager : MonoBehaviour
{
    [SerializeField] UINavigator nav;

    [Header("Input Settings")]
    [SerializeField] float inputRepeatDelay = 0.35f;
    [SerializeField] float inputRepeatRate = 0.1f;

    [Header("Input Actions")]
    [SerializeField] InputActionReference navigateAction;
    [SerializeField] InputActionReference submitAction;
    [SerializeField] InputActionReference cancelAction;
    [SerializeField] InputActionReference tabAction;
    [SerializeField] InputActionReference modifyAction;

    bool modifyHeld;

    public InputActionReference NavigateAction => navigateAction;
    public InputActionReference SubmitAction => submitAction;
    public InputActionReference CancelAction => cancelAction;
    public InputActionReference TabAction => tabAction;
    public InputActionReference ModifyAction => modifyAction;

    private void OnEnable()
    {
        EnableActions();
        EnableInputs();

        EventBus.Subscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Subscribe<EnableNavigationEvent>(OnEnableNavigation);
    }

    private void OnDisable()
    {
        DisableInputs();

        EventBus.Unsubscribe<DisableNavigationEvent>(OnDisableNavigation);
        EventBus.Unsubscribe<EnableNavigationEvent>(OnEnableNavigation);
    }

    private void Update()
    {
        ProcessNavInput();
    }

    private void OnDisableNavigation(DisableNavigationEvent e)
    {
        DisableMainInputs();
    }

    private void OnEnableNavigation(EnableNavigationEvent e)
    {
        EnableMainInputs();
    }


    private void EnableActions()
    {
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();
        tabAction.action.Enable();
        modifyAction.action.Enable();
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
        tabAction.action.performed += OnTabPerformed;
        modifyAction.action.performed += OnModifyPerformed;
    }

    private void DisableMainInputs()
    {
        allowMovement = false;
        submitAction.action.performed -= OnSubmitPerformed;
        tabAction.action.performed -= OnTabPerformed;
        modifyAction.action.performed -= OnModifyPerformed;
    }


    bool allowMovement;
    float nextRepeatTime;
    bool lastModifyHeld;
    Vector2 lastMoveInput;
    void ProcessNavInput()
    {
        if (!allowMovement) return;

        Vector2 raw = navigateAction.action.ReadValue<Vector2>();
        //Vector2 input = FilterDiagonalTransitions(raw);
        Vector2 input = raw;

        if (input == Vector2.zero)
        {
            lastMoveInput = Vector2.zero;
            nextRepeatTime = 0f;
            return;
        }

        bool newInput = input != lastMoveInput;
        if (lastModifyHeld != modifyHeld) newInput = true;

        if (newInput || Time.time >= nextRepeatTime)
        {
            Vector2 dir = input;

            dir.x = Mathf.RoundToInt(dir.x);
            dir.y = Mathf.RoundToInt(dir.y);
            if (modifyHeld)
            {
                dir.x *= 3;
                dir.y *= 3;
            }

            nav.TriggerItemNav(dir);

            if (modifyHeld) nav.TriggerItemNav(dir); // double trigger when modify held (dir mag doesnt matter for item mode)
            

            nextRepeatTime = Time.time + (newInput ? inputRepeatDelay : inputRepeatRate);
            lastMoveInput = input;
        }

        lastModifyHeld = modifyHeld;
    }


    private void OnSubmitPerformed(InputAction.CallbackContext ctx) => EventBus.Publish(new SubmitInputEvent { });

    private void OnCancelPerformed(InputAction.CallbackContext ctx) => EventBus.Publish(new CancelInputEvent { });

    private void OnTabPerformed(InputAction.CallbackContext ctx) => EventBus.Publish(new TabInputEvent { modHeld = modifyHeld });

    private void OnModifyPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();

        modifyHeld = input == 1;

        //EventBus.Publish(new ModifyInputEvent { held = modifyHeld });
    }
}
