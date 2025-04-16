using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Game Data/Weapon")]
public class WeaponData : ScriptableObject
{
    [Tooltip("Bullet PREFAB fire from this weapon.")]
    public Bullet bullet;

    [Tooltip("Rate of fire in bullets per second.")]
    public float fireRate = 10;

    [Tooltip("False will make semi-automatic, required re-press for another fire. \n" +
             "Automatic lets ship hold down input to continuously fire.")]
    public bool automatic = true;

    [Tooltip("Initial forward velocity the fired bullet will have.")]
    public float muzzleVelocity = 10;

    [Tooltip("Multiplier that will be applied to the damage of the fired bullet.")]
    public float damageMultiplier = 1;

    [Tooltip("How many bullets can be fired before reload is required")]
    public int ammo = 30;

    [Tooltip("Time it takes to reload weapon")]
    public float reloadTime = 3;
}

