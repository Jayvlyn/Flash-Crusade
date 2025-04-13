using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Ship ship;
	private ShipInputData inputData = new ShipInputData();

	#region INPUTS

    public void OnFireWeapon1(InputAction.CallbackContext context)
    {
		if (context.started)
		{
			inputData.holdingFireWeapon1 = true;
			ship.currentInputData = inputData;
		}
		else if (context.canceled)
		{
			inputData.holdingFireWeapon1 = false;
			ship.currentInputData = inputData;
		}
	}
	
	public void OnFireWeapon2(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			inputData.holdingFireWeapon2 = true;
			ship.currentInputData = inputData;
		}
		else if (context.canceled)
		{
			inputData.holdingFireWeapon2 = false;
			ship.currentInputData = inputData;
		}
	}

	public void OnFireWeapon3(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			inputData.holdingFireWeapon3 = true;
			ship.currentInputData = inputData;
		}
		else if (context.canceled)
		{
			inputData.holdingFireWeapon3 = false;
			ship.currentInputData = inputData;
		}
	}


	public void OnThrust(InputAction.CallbackContext context) // default W-A-S-D
	{
		if (context.performed || context.canceled)
		{
			inputData.thrustInput = context.ReadValue<Vector2>();
			ship.currentInputData = inputData;

			if (inputData.thrustInput != Vector2.zero && inputData.holdingBoost) ship.Boost(true);
			else if (inputData.thrustInput == Vector2.zero && inputData.holdingBoost) ship.Boost(false);
		}
	}

	public void OnBoost(InputAction.CallbackContext context) // default K
	{
		if (context.started)
		{
			inputData.holdingBoost = true;
			if(inputData.thrustInput != Vector2.zero) ship.Boost(true);
			ship.currentInputData = inputData;
		}
		else if (context.canceled)
		{
			inputData.holdingBoost = false;
			ship.Boost(false);
			ship.currentInputData = inputData;
		}
	}


	public void OnTurn(InputAction.CallbackContext context) // default J-L
	{
		if (context.performed || context.canceled)
		{
			inputData.turnInput = context.ReadValue<float>();
			ship.currentInputData = inputData;
		}
	}
	#endregion

	#region INPUT HOLD HANDLING

	private void OnEnable()
	{
		Application.focusChanged += OnFocusChanged;
	}

	private void OnDisable()
	{
		Application.focusChanged -= OnFocusChanged;
	}

	private void OnFocusChanged(bool hasFocus)
	{
		if (!hasFocus) // lost focus
		{
			inputData.Clear();
		}
	}

	#endregion
}
