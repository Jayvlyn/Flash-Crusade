﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Fleet
{
	public List<AIAgent> ships;
	private FleetFormation activeFleetFormation;
	private Ship leader;
	public Vector2[] localFleetPositions; // positions of fleet relative to leader
	public float shipSpacing;

	public Fleet(Ship leader, float shipSpacing = 5f)
	{
		this.leader = leader;
		this.shipSpacing = shipSpacing;
		localFleetPositions = new Vector2[0];
		ships = new List<AIAgent>();
	}

	public Fleet(Ship leaderTransform, List<AIAgent> ships, float shipSpacing = 5f)
	{
		this.leader = leaderTransform;
		this.shipSpacing = shipSpacing;
		localFleetPositions = new Vector2[0];
		this.ships = ships;
	}

	private void AddToFleet(AIAgent ship)
	{
		ships.Add(ship);
		ship.Leader = leader;
	}

	private void RemoveFromFleet(AIAgent ship)
	{
		ships.Remove(ship);
		ship.Leader = null;
	}

	public void SetFleetFormation(FleetFormation formation)
	{
		activeFleetFormation = formation;

		UpdateLocalFleetPositions();
	}

	public void GiveFleetLatestPoints()
	{
		float angle = leader.transform.eulerAngles.z;
		Quaternion rotation = Quaternion.Euler(0, 0, angle);

		for (int i = 0; i < localFleetPositions.Length; i++)
		{
			localFleetPositions[i] = rotation * localFleetPositions[i];

			if (i >= ships.Count) continue;

			Vector2 worldPos = (Vector2)leader.transform.position + localFleetPositions[i];

			ships[i].MoveTarget = worldPos;
		}
	}

	public void UpdateLocalFleetPositions()
	{
		//localFleetPositions = new Vector2[50];
		localFleetPositions = new Vector2[ships.Count];
		switch (activeFleetFormation)
		{
			case FleetFormation.VIC:
				SetVicFormation();
				break;
			case FleetFormation.REVERSE_VIC:
				SetVicFormation(true);
				break;
			case FleetFormation.ECHELON:
				SetEchelonFormation();
				break;
			case FleetFormation.LINE_ABREAST:
				SetLineAbreastFormation();
				break;
			case FleetFormation.WEDGE:
				SetWedgeFormation();
				break;
			case FleetFormation.COLUMN:
				SetColumnFormation();
				break;
			case FleetFormation.BUBBLE:
				SetBubbleFormation();
				break;
			case FleetFormation.SHIELD:
				SetShieldFormation();
				break;
		}
		GiveFleetLatestPoints();
	}

	#region FORMATION METHODS
	private void SetVicFormation(bool reversed = false)
	{
		if (localFleetPositions.Length <= 0) return;
		for (int i = 0; i < localFleetPositions.Length; i++)
		{
			int x = Utilities.RoundHalfUp((i + 1) * 0.5f); // increment every 2
			int y = reversed ? x : -x;
			if (i % 2 == 1) x *= -1; // odd x is on left
			localFleetPositions[i] = new Vector2(x * shipSpacing, y * shipSpacing);
		}
	}

	private void SetEchelonFormation()
	{
		if (localFleetPositions.Length <= 0) return;
		for (int i = 0; i < localFleetPositions.Length; i++)
		{
			int x = i + 1;
			int y = -x;
			localFleetPositions[i] = new Vector2(x * shipSpacing, y * shipSpacing);
		}
	}

	private void SetLineAbreastFormation()
	{
		if (localFleetPositions.Length <= 0) return;
		for (int i = 0; i < localFleetPositions.Length; i++)
		{
			int x = Utilities.RoundHalfUp((i + 1) * 0.5f); // increment every 2
			if (i % 2 == 1) x *= -1; // odd x is on left
			localFleetPositions[i] = new Vector2(x * shipSpacing, 0);
		}
	}

	private void SetColumnFormation()
	{
		if (localFleetPositions.Length <= 0) return;
		for (int i = 0; i < localFleetPositions.Length; i++)
		{
			int y = -(i + 1);
			localFleetPositions[i] = new Vector2(0, y * shipSpacing);
		}
	}

	private void SetWedgeFormation()
	{
		if (localFleetPositions.Length <= 0) return;
		int placed = 0;
		int layer = 2; // start at second layer since leader is not included

		while (placed < localFleetPositions.Length)
		{
			float y = -layer * shipSpacing; // layer 2 with 2 ships offset back by 10 with shipSpacing of 5, so move everything up by shipSpacing at end

			for (int i = 0; i < layer; i++) // foreach ship on layer
			{
				if (placed >= localFleetPositions.Length) break;  // break out when not enough ships to finish layer

				float x = (i - (layer - 1) / 2f) * shipSpacing; // Take the current ship's index in the row, subtract the row's center, then scale by shipSpacing to get its position in world space
				localFleetPositions[placed] = new Vector2(x, y + shipSpacing);
				placed++;
			}

			layer++;
		}
	}

	private void SetBubbleFormationSimple()
	{
		if (localFleetPositions.Length <= 0) return;
		int count = localFleetPositions.Length;

		float circumference = shipSpacing * count;
		float radius = circumference / (2f * Mathf.PI);

		for (int i = 0; i < count; i++)
		{
			float angle = (2f * Mathf.PI / count) * i;
			float x = Mathf.Cos(angle) * radius;
			float y = Mathf.Sin(angle) * radius;
			localFleetPositions[i] = new Vector2(x, y);
		}
	}

	private void SetBubbleFormation()
	{
		if (localFleetPositions.Length <= 0) return;
		int totalShips = localFleetPositions.Length;

		float minSpacing = shipSpacing;
		float ringSpacing = shipSpacing * 1.5f;
		List<int> shipsPerRing = new List<int>();
		List<float> radii = new List<float>();

		int shipsRemaining = totalShips;
		int ringIndex = 0;

		while (shipsRemaining > 0)
		{
			// Estimate how many ships fit around a ring with given spacing
			float radius = (ringIndex + 1) * ringSpacing;
			float circumference = 2f * Mathf.PI * radius;
			int maxShipsInRing = Mathf.FloorToInt(circumference / minSpacing);

			// If too few ships remain to make another decent ring, dump them into the current one
			if (shipsRemaining < 8 && shipsPerRing.Count > 0)
			{
				shipsPerRing[shipsPerRing.Count - 1] += shipsRemaining;
				shipsRemaining = 0;
				break;
			}

			int shipsInThisRing = Mathf.Min(shipsRemaining, maxShipsInRing);
			shipsPerRing.Add(shipsInThisRing);
			radii.Add(radius);
			shipsRemaining -= shipsInThisRing;
			ringIndex++;
		}

		int placed = 0;
		for (int ring = 0; ring < shipsPerRing.Count; ring++)
		{
			int count = shipsPerRing[ring];
			float radius = radii[ring];
			float angleStep = 2f * Mathf.PI / count;

			for (int i = 0; i < count; i++)
			{
				float angle = angleStep * i;
				float x = Mathf.Cos(angle) * radius;
				float y = Mathf.Sin(angle) * radius;
				localFleetPositions[placed++] = new Vector2(x, y);
			}
		}
	}

	private void SetShieldFormation()
	{
		if (localFleetPositions.Length <= 0) return;
		for (int i = 0; i < localFleetPositions.Length; i++)
		{
			int layer = (i / 11) + 1;
			int posInLayer = i % 11;

			int x = Utilities.RoundHalfUp((posInLayer) * 0.5f); // increment every 2
			float y = -x * 0.2f;
			if (i % 2 == 1) x *= -1; // odd x is on left
			localFleetPositions[i] = new Vector2(x * shipSpacing, y * shipSpacing + shipSpacing * layer);
		}
	}
	#endregion
}

public enum FleetFormation
{
	VIC,            // ^ Shape
	REVERSE_VIC,    // V Shape
	ECHELON,        // Diagonal Line
	LINE_ABREAST,   // Wall
	WEDGE,          // Triangle
	COLUMN,         // Line / Rectangle
	BUBBLE,			// Circle around leader
	SHIELD,			// wide ^ shield in front of leader. Multilayers with enough ships
}
