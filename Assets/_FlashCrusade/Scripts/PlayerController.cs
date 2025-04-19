using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Ship ship;

	private void Start()
	{
		ship.inputData.holdingFireWeapons = new bool[3];
	}

	#region INPUTS

	public void OnFireWeapon1(InputAction.CallbackContext context)
    {
		if (context.started)
		{
			ship.inputData.holdingFireWeapons[0] = true;
		}
		else if (context.canceled)
		{
			ship.inputData.holdingFireWeapons[0] = false;
		}
	}
	
	public void OnFireWeapon2(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			ship.inputData.holdingFireWeapons[1] = true;
		}
		else if (context.canceled)
		{
			ship.inputData.holdingFireWeapons[1] = false;
		}
	}

	public void OnFireWeapon3(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			ship.inputData.holdingFireWeapons[2] = true;
		}
		else if (context.canceled)
		{
			ship.inputData.holdingFireWeapons[2] = false;
		}
	}


	public void OnThrust(InputAction.CallbackContext context) // default W-A-S-D
	{
		if (context.performed || context.canceled)
		{
			ship.inputData.thrustInput = context.ReadValue<Vector2>();

			if (ship.inputData.thrustInput != Vector2.zero && ship.inputData.holdingBoost) ship.Boost(true);
			else if (ship.inputData.thrustInput == Vector2.zero && ship.inputData.holdingBoost) ship.Boost(false);
		}
	}

	public void OnBoost(InputAction.CallbackContext context) // default K
	{
		if (context.started)
		{
			ship.inputData.holdingBoost = true;
			if(ship.inputData.thrustInput != Vector2.zero) ship.Boost(true);
		}
		else if (context.canceled)
		{
			ship.inputData.holdingBoost = false;
			ship.Boost(false);
		}
	}


	public void OnTurn(InputAction.CallbackContext context) // default J-L
	{
		if (context.performed || context.canceled)
		{
			ship.inputData.turnInput = context.ReadValue<float>();
		}
	}

	public void OnSetFleetFormation(InputAction.CallbackContext context)
	{
		if (context.started)
		{

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
			ship.inputData.Clear();
		}
	}

	#endregion
}
