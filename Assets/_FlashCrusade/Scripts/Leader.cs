
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leader : MonoBehaviour
{
	public Fleet fleet;
	public float fleetShipSpacing = 5f;

	private bool freeFly = false;

	private void Start()
	{
		fleet = new Fleet(this.transform, fleetShipSpacing);

		Ally[] allyComps = FindObjectsByType<Ally>(FindObjectsSortMode.None);
		fleet.ships = allyComps.Cast<AIAgent>().ToList();
	}

	public void UpdateFleetInputs(ShipInputData inputData)
	{
		for (int i = 0; i < fleet.ships.Count; i++)
		{
			fleet.ships[i].Ship.inputData = inputData;
		}
	}

	#region COMMANDS

	public void ToggleFreeFly()
	{
		freeFly = !freeFly;

		foreach(AIAgent ship in fleet.ships)
		{
			ship.FreeFly = freeFly;
		}
	}

	#endregion

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying) return;
		Gizmos.color = Color.cyan;
		foreach (Vector2 position in fleet.localFleetPositions)
		{
			Gizmos.DrawSphere(transform.position + (Vector3)position, 0.2f);
		}
	}
}
