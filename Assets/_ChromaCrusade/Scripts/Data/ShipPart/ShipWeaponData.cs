using UnityEngine;

public class ShipWeaponData : ShipPartData
{
    public int damage;
    public float spread;
    public float fireRate;


    public override PartType PartType => PartType.Weapon;
}
