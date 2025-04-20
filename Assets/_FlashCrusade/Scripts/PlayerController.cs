using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Ship ship;
    [SerializeField] private Leader leader;

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
		if (context.started || context.canceled)
		{
			ship.inputData.turnInput = context.ReadValue<float>();
		}
	}

	public void OnToggleFreeFly(InputAction.CallbackContext context)
	{
		if(context.started)
		{
			leader.ToggleFreeFly();
		}
	}

	public void OnFormation1(InputAction.CallbackContext context)
	{
		if(context.started)
		{
			leader.fleet.SetFleetFormation(FleetFormation.VIC);
		}
	}

	public void OnFormation2(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			leader.fleet.SetFleetFormation(FleetFormation.REVERSE_VIC);
		}
	}

	public void OnFormation3(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			leader.fleet.SetFleetFormation(FleetFormation.ECHELON);
		}
	}

	public void OnFormation4(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			leader.fleet.SetFleetFormation(FleetFormation.LINE_ABREAST);
		}
	}

	public void OnFormation5(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			leader.fleet.SetFleetFormation(FleetFormation.WEDGE);
		}
	}

	public void OnFormation6(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			leader.fleet.SetFleetFormation(FleetFormation.COLUMN);
		}
	}

	public void OnFormation7(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			leader.fleet.SetFleetFormation(FleetFormation.BUBBLE);
		}
	}

	public void OnFormation8(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			leader.fleet.SetFleetFormation(FleetFormation.SHIELD);
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
