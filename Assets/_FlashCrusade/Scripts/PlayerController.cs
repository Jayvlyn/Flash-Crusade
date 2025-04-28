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
			ship.InputData.holdingFireWeapons[0] = true;
		}
		else if (context.canceled)
		{
			ship.InputData.holdingFireWeapons[0] = false;
		}
	}
	
	public void OnFireWeapon2(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			ship.InputData.holdingFireWeapons[1] = true;
		}
		else if (context.canceled)
		{
			ship.InputData.holdingFireWeapons[1] = false;
		}
	}

	public void OnFireWeapon3(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			ship.InputData.holdingFireWeapons[2] = true;
		}
		else if (context.canceled)
		{
			ship.InputData.holdingFireWeapons[2] = false;
		}
	}


	public void OnThrust(InputAction.CallbackContext context) // default W-A-S-D
	{
		if (context.performed || context.canceled)
		{
			ship.InputData.thrustInput = context.ReadValue<Vector2>();

			if (ship.InputData.isMovingOrTurning && ship.InputData.holdingBoost) ship.Boost(true);
			else if (!ship.InputData.isMovingOrTurning && ship.InputData.holdingBoost) ship.Boost(false);

			ship.UpdateActiveThrusters();
		}
	}

	public void OnBoost(InputAction.CallbackContext context) // default K
	{
		if (context.started)
		{
			ship.InputData.holdingBoost = true;
			if(ship.InputData.isMovingOrTurning) ship.Boost(true);

			ship.UpdateActiveThrusters();
		}
		else if (context.canceled)
		{
			ship.InputData.holdingBoost = false;

			ship.Boost(false);

			ship.UpdateActiveThrusters();
		}
	}


	public void OnTurn(InputAction.CallbackContext context) // default J-L
	{
		if (context.started || context.canceled)
		{
			ship.InputData.turnInput = context.ReadValue<float>();

            if (ship.InputData.isMovingOrTurning && ship.InputData.holdingBoost) ship.Boost(true);
            else if (!ship.InputData.isMovingOrTurning && ship.InputData.holdingBoost) ship.Boost(false);

			if(!leader.FreeFlyOn)
			{
				foreach(AIAgent ship in leader.fleet.ships)
				{
					ship.Ship.InputData.turnInput = this.ship.InputData.turnInput;
				}
			}
			ship.UpdateActiveThrusters();

			if(context.canceled)
			{
                leader.fleet.UpdateLocalFleetPositions();
                leader.UpdateFleetMoveTargets();
            }
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
			ship.InputData.Clear();
		}
	}

	#endregion
}
