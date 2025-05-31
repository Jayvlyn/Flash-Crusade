
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leader : MonoBehaviour
{
	public FleetManagerFacade fleetManager;
	public Ship ship;

	[SerializeField] private float fleetShipSpacing = 5f;

	private bool freeFly = false;
	public bool FreeFlyOn { get { return freeFly; } }

	private void Start()
	{
		fleetManager = new FleetManagerFacade(this, fleetShipSpacing);
    }

	private void Update()
	{
		fleetManager.Update();
	}

	#region COMMANDS

	public void ToggleFreeFly()
	{
		freeFly = !freeFly;

		foreach(AIAgent ship in fleetManager.fleet.ships)
		{
			if(freeFly)
			{
				ship.sm.ChangeState(ship.freeFlyState);
			}
			else
			{
				ship.sm.ChangeState(ship.followState);
			}
		}
	}

	#endregion

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying) return;
		Gizmos.color = Color.cyan;
		foreach (Vector2 position in fleetManager.fleet.localFleetPositions)
		{
			Gizmos.DrawSphere(transform.position + (Vector3)position, 10);
		}
	}
}
