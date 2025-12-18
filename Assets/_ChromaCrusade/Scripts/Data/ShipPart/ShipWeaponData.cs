using UnityEngine;

public class ShipWeaponData : ShipPartData
{
    public int damage;
    public int projectileSpeed;
    public float fireRate;

    public override PartType PartType => PartType.Weapon;
}
