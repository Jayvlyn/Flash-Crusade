using System.Collections.Generic;
using UnityEngine;

public class Fleet
{
	private FleetFormation activeFleetFormation;
	private Transform leaderTransform;
	public List<AIAgent> ships;

	public Fleet(Transform leaderTransform)
	{
		this.leaderTransform = leaderTransform;
		ships = new List<AIAgent>();
	}

	public Fleet(Transform leaderTransform, List<AIAgent> ships)
	{
		this.leaderTransform = leaderTransform;
		this.ships = ships;
	}

	private void AddToFleet(AIAgent ship)
	{
		ships.Add(ship);
		ship.Leader = leaderTransform;
	}

	private void RemoveFromFleet(AIAgent ship)
	{
		ships.Remove(ship);
		ship.Leader = null;
	}

	public void SetFleetFormation(FleetFormation formation)
	{
		activeFleetFormation = formation;
		switch (activeFleetFormation)
		{
			case FleetFormation.VIC:
				break;
			case FleetFormation.REVERSE_VIC:
				break;
			case FleetFormation.ECHELON:
				break;
			case FleetFormation.LINE_ABREAST:
				break;
			case FleetFormation.WEDGE:
				break;
			case FleetFormation.COLUMN:
				break;
		}
	}
}

public enum FleetFormation
{
	VIC,            // ^ Shape
	REVERSE_VIC,    // V Shape
	ECHELON,        // Diagonal Line
	LINE_ABREAST,   // Wall
	WEDGE,          // Triangle
	COLUMN,         // Line / Rectangle
}
