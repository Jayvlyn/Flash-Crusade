using NaughtyAttributes;

public class ShipWeaponData : ShipPartData
{
    public int damage;
    public float spread;
    public float fireRate; // becomes damage rate for beam
    public FirePoint[] firePoints;
    public override PartType PartType => PartType.Weapon;

    public FireType fireType;
    public enum FireType
    {
        Projectile = 1,
        Beam = 2,
        Wave = 3,
    }
    bool IsProjectile() => fireType == FireType.Projectile;
    bool IsBeam() => fireType == FireType.Beam;
    bool IsWave() => fireType == FireType.Wave;

    // Projectile Weapon Data
    [ShowIf(nameof(IsProjectile))]
    public ProjectileData projectile;

    [ShowIf(nameof(IsBeam))]
    public float beamThickness;
    [ShowIf(nameof(IsBeam))]
    public float chargeTime;

    [ShowIf(nameof(IsWave))]
    public float growSpeed;
}