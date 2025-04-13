using UnityEngine;

public class ShipInputData
{
	public bool holdingFireWeapon1 = false;
	public bool holdingFireWeapon2 = false;
	public bool holdingFireWeapon3 = false;
	public Vector2 thrustInput = Vector2.zero;
	public bool holdingBoost = false;
	public float turnInput = 0;

	public void Clear()
    {
		holdingFireWeapon1 = false;
		holdingFireWeapon2 = false;
		holdingFireWeapon3 = false;
		holdingBoost = false;
		thrustInput = Vector2.zero;
		turnInput = 0;
	}
}
