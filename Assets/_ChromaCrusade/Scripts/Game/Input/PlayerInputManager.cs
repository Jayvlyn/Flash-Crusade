using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] InputActionReference thrustAction;
    [SerializeField] InputActionReference turnAction;
    [SerializeField] InputActionReference turboAction;
    [SerializeField] InputActionReference brakeAction;
    [SerializeField] InputActionReference fireAction;

    public InputActionReference ThrustAction => thrustAction;
    public InputActionReference TurnAction => turnAction;
    public InputActionReference TurboAction => turboAction;
    public InputActionReference BrakeAction => brakeAction;
    public InputActionReference FireAction => fireAction;

    #region Lifecycle
    private void OnEnable()
    {
        EnableActions();
        SubscribeInputs();
    }
    #endregion

    #region Management
    void EnableActions()
    {
        thrustAction.action.Enable();
        turnAction.action.Enable();
        turboAction.action.Enable();
        brakeAction.action.Enable();
        fireAction.action.Enable();
    }

    void SubscribeInputs()
    {
        thrustAction.action.performed += OnThrustPerformed;
        turnAction.action.performed += OnTurnPerformed;
        turboAction.action.performed += OnTurboPerformed;
        brakeAction.action.performed += OnBrakePerformed;
        fireAction.action.performed += OnFirePerformed;
    }

    void UnsubscribeInputs()
    {
        thrustAction.action.performed -= OnThrustPerformed;
        turnAction.action.performed -= OnTurnPerformed;
        turboAction.action.performed -= OnTurboPerformed;
        brakeAction.action.performed -= OnBrakePerformed;
        fireAction.action.performed -= OnFirePerformed;
    }
    #endregion

    #region Responses
    void OnThrustPerformed(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        Debug.Log($"Thrust {input}");
    }

    void OnTurnPerformed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        Debug.Log($"Turn {input}");
    }

    void OnTurboPerformed(InputAction.CallbackContext ctx)
    {
        bool input = ctx.performed;
        Debug.Log($"Turbo {input}");
    }

    void OnBrakePerformed(InputAction.CallbackContext ctx)
    {
        bool input = ctx.performed;
        Debug.Log($"Brake {input}");

    }

    void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        bool input = ctx.performed;
        Debug.Log($"Fire {input}");

    }
    #endregion
}
