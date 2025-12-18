using UnityEngine;

public class ShipWeaponData : ShipPartData
{
    public int damage;
    public int projectileSpeed;
    public float fireRate;

    public override PartType PartType => PartType.Weapon;

    //public override void Apply(ImporterPart importer)
    //{
    //    base.Apply(importer);
    //    damage = importer.damage;
    //    projectileSpeed = importer.projectileSpeed;
    //    fireRate = importer.fireRate;
    //}
}
