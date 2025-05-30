
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leader : MonoBehaviour
{
	public float fleetPosTick = 0.5f;
	private float fleetPosTickTimer;


	public Ship ship;
	public Fleet fleet;
	public float fleetShipSpacing = 5f;

	private bool freeFly = false;
	public bool FreeFlyOn { get { return freeFly; } }

	private void Start()
	{
		fleet = new Fleet(this.ship, fleetShipSpacing);

		Ally[] allyComps = FindObjectsByType<Ally>(FindObjectsSortMode.None);
		fleet.ships = allyComps.Cast<AIAgent>().ToList();

        fleet.UpdateLocalFleetPositions();

        //UpdateFleetMoveTargets();
    }

	private void Update()
	{
		if(fleetPosTickTimer > 0)
		{
			fleetPosTickTimer -= Time.deltaTime;
		}
		else
		{

			fleetPosTickTimer = fleetPosTick;
		}
		fleet.UpdateLocalFleetPositions(); // move back into tick later
	}

	public void UpdateFleetMoveTargets()
	{
		for (int i = 0; i < fleet.ships.Count; i++)
		{
			Vector2 worldPos = (Vector2)transform.position + fleet.localFleetPositions[i];

			fleet.ships[i].MoveTarget = worldPos;
		}
	}

	#region COMMANDS

	public void ToggleFreeFly()
	{
		Debug.Log("Toggle free fly");
		freeFly = !freeFly;

		foreach(AIAgent ship in fleet.ships)
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
		foreach (Vector2 position in fleet.localFleetPositions)
		{
			Gizmos.DrawSphere(transform.position + (Vector3)position, 10);
		}
	}
}
