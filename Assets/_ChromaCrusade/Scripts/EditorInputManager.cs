using UnityEngine;
using UnityEngine.InputSystem;

public class EditorInputManager : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] float inputRepeatDelay = 0.35f;
    [SerializeField] float inputRepeatRate = 0.1f;

    [Header("Input Actions")]
    [SerializeField] InputActionReference navigateAction;
    [SerializeField] InputActionReference submitAction;
    [SerializeField] InputActionReference cancelAction;
    [SerializeField] InputActionReference modeAction;
    [SerializeField] InputActionReference resetAction;
    [SerializeField] InputActionReference zoomAction;
    [SerializeField] InputActionReference undoAction;
    [SerializeField] InputActionReference redoAction;
    [SerializeField] InputActionReference rotateAction;
    [SerializeField] InputActionReference flipAction;
    [SerializeField] InputActionReference modifyAction;
    [SerializeField] InputActionReference deleteAction;

    #region Lifecycle

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
        ProcessUndoRedoRepeat();
        ProcessNavInput();
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
        deleteAction.action.Enable();
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
        modeAction.action.performed += OnModePerformed;
        resetAction.action.performed += OnResetPerformed;
        zoomAction.action.performed += OnZoomPerformed;
        undoAction.action.performed += OnUndoPerformed;
        redoAction.action.performed += OnRedoPerformed;
        rotateAction.action.performed += OnRotatePerformed;
        flipAction.action.performed += OnFlipPerformed;
        modifyAction.action.performed += OnModifyPerformed;
        deleteAction.action.performed += OnDeletePerformed;
    }

    private void DisableMainInputs()
    {
        allowMovement = false;
        submitAction.action.performed -= OnSubmitPerformed;
        modeAction.action.performed -= OnModePerformed;
        resetAction.action.performed -= OnResetPerformed;
        zoomAction.action.performed -= OnZoomPerformed;
        undoAction.action.performed -= OnUndoPerformed;
        redoAction.action.performed -= OnRedoPerformed;
        rotateAction.action.performed -= OnRotatePerformed;
        flipAction.action.performed -= OnFlipPerformed;
        modifyAction.action.performed -= OnModifyPerformed;
        deleteAction.action.performed -= OnDeletePerformed;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            modifyHeld = modifyAction.action.IsPressed();
            undoHeld = undoAction.action.IsPressed();
            redoHeld = undoAction.action.IsPressed();
        }
        else
        {
            modifyHeld = false;
            undoHeld = false;
            redoHeld = false;
        }
    }

    #endregion

    #region Input Actions

    private void OnSubmitPerformed(InputAction.CallbackContext ctx) => EventBus.Publish(new SubmitInputEvent { });

    private void OnCancelPerformed(InputAction.CallbackContext ctx) => EventBus.Publish(new CancelInputEvent { });
    
    private void OnModePerformed(InputAction.CallbackContext ctx) => EventBus.Publish(new ModeInputEvent { });
    
    private void OnResetPerformed(InputAction.CallbackContext ctx) => EventBus.Publish(new ResetInputEvent { });
    
    private void OnDeletePerformed(InputAction.CallbackContext ctx) => EventBus.Publish(new DeleteInputEvent { });

    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();

        ZoomDirection zoomDir;
        if (input < 0.5f) zoomDir = ZoomDirection.In;
        else if (input > 0.5f) zoomDir = ZoomDirection.Out;
        else return;

        EventBus.Publish(new ZoomInputEvent { zoomDirection = zoomDir });
    }

    private void OnUndoPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        undoHeld = input == 1;
    }

    private void OnRedoPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        redoHeld = input == 1;
    }

    private void OnRotatePerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        if (input == 0) return;

        RotationDirection rotDir = (input == 1) ? 
            RotationDirection.Clockwise : 
            RotationDirection.CounterClockwise;

        EventBus.Publish(new RotateInputEvent { rotationDirection = rotDir });
    }

    private void OnFlipPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        if (input == 0) return;
        FlipAxis axis = (input == 1) ? FlipAxis.Vertical : FlipAxis.Horizontal;
        EventBus.Publish(new FlipInputEvent { flipAxis = axis });
    }

    private void OnModifyPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();

        modifyHeld = input == 1;

        EventBus.Publish(new ModifyInputEvent { held = modifyHeld });
    }

    #endregion

    #region Per-frame Processes

    bool modifyHeld;
    bool undoHeld;
    bool redoHeld;
    float undoNextTime;
    float redoNextTime;
    private void ProcessUndoRedoRepeat()
    {
        if (undoHeld && !modifyHeld)
        {
            if (Time.time >= undoNextTime)
            {
                EventBus.Publish(new UndoInputEvent { });
                undoNextTime = Time.time + (undoNextTime == 0f ? inputRepeatDelay : inputRepeatRate);
            }
        }
        else undoNextTime = 0f;

        if (redoHeld || (undoHeld && modifyHeld))
        {
            if (Time.time >= redoNextTime)
            {
                EventBus.Publish(new RedoInputEvent { });
                redoNextTime = Time.time + (redoNextTime == 0f ? inputRepeatDelay : inputRepeatRate);
            }
        }
        else redoNextTime = 0f;
    }

    bool allowMovement;
    float nextRepeatTime;
    bool lastModifyHeld;
    Vector2 lastMoveInput;
    void ProcessNavInput()
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
        if (lastModifyHeld != modifyHeld) newInput = true;

        if (newInput || Time.time >= nextRepeatTime)
        {
            EventBus.Publish(new NavigateInputEvent { dir = input });

            nextRepeatTime = Time.time + (newInput ? inputRepeatDelay : inputRepeatRate);
            lastMoveInput = input;
        }

        lastModifyHeld = modifyHeld;
    }

    #endregion

    #region Input Helpers

    Vector2 prevStableInput = Vector2.zero;
    Vector2 pendingCardinal = Vector2.zero;
    Vector2 pendingDiagonal = Vector2.zero;
    float pendingTime;
    int stuckHits;
    int stuckHitThreshold = 5; // could make this frame-dependent like pendingWindow
    float cardinalHoldTime;
    float diagonalHoldTime;
    Vector2 FilterDiagonalTransitions(Vector2 raw)
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
                if (cardinalHoldTime > 0.2f || diagonalHoldTime > 0.2f)
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

    bool IsDiagonal(Vector2 v)
    {
        return Mathf.Abs(v.x) > 0.1f && Mathf.Abs(v.y) > 0.1f;
    }

    bool IsCardinal(Vector2 v)
    {
        return Mathf.Abs(v.x) > 0.1f ^ Mathf.Abs(v.y) > 0.1f;
    }

    bool IsNeutral(Vector2 v)
    {
        return v.sqrMagnitude < 0.01f;
    }

    #endregion
}
