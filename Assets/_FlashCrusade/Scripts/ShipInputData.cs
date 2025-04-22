using System.Runtime.CompilerServices;
using UnityEngine;

public class ShipInputData
{
	public bool[] holdingFireWeapons;
	public Vector2 thrustInput = Vector2.zero;
	public bool holdingBoost = false;
	public float turnInput = 0;
	public int fleetFormation = 0;

	public bool isMovingOrTurning { get {  return (thrustInput != Vector2.zero || turnInput != 0); } }

	public void Clear()
    {
		for(int i = 0; i < holdingFireWeapons.Length; i++)
		{
			holdingFireWeapons[i] = false;
		}
		holdingBoost = false;
		thrustInput = Vector2.zero;
		turnInput = 0;
		fleetFormation = 0;
	}
}
