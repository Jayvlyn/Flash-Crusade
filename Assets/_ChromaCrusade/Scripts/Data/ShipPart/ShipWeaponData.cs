using UnityEngine;
using System.Collections.Generic;

public class ShipWeaponData : ShipPartData
{
    public int damage;
    public float spread;
    public float fireRate;
    public FireType fireType;
    public FirePoint[] firePoints;

    public override PartType PartType => PartType.Weapon;
}
