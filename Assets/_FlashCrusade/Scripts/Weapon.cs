using UnityEngine;

public class Weapon : MonoBehaviour
{
	[Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;
	[SerializeField] private Vector3 firepoint;

	[Header("Arrow Gizmo")]
	[SerializeField] private Color gizmoColor = Color.cyan;
	[SerializeField] private float arrowLength = 10f;

	private float FireCooldown { get { return 1 / weaponData.fireRate; } }
	private Vector3 FirepointLocalPosition {  get { return new Vector3(firepoint.x, firepoint.y, 0f); } }
	private float FirepointRotation { get { return transform.eulerAngles.z + firepoint.z; } }
	private float fireRateTimer;

	private int currentAmmo;
	private float reloadTimer;

	private Vector2 shipVelocity;

    private void Start()
    {
		currentAmmo = weaponData.ammo;
    }

    private void Update()
	{
		if (fireRateTimer > 0) fireRateTimer -= Time.deltaTime;
		if (reloadTimer > 0)
		{ // reloading
			reloadTimer -= Time.deltaTime;
			if(reloadTimer <= 0)
			{ // finish reload
				currentAmmo = weaponData.ammo;
			}
		}
	}

	public void Fire()
	{
		if(fireRateTimer <= 0 && currentAmmo > 0)
		{
            Vector3 worldPos = transform.TransformPoint(FirepointLocalPosition);

            Quaternion rot = Quaternion.Euler(0, 0, FirepointRotation);

            Bullet bullet = Instantiate(weaponData.bullet, worldPos, rot);

			Vector2 initialBulletVelocity = ((Vector2)bullet.transform.up * weaponData.muzzleVelocity);// + shipVelocity;

			bullet.SetInitialVelocity(initialBulletVelocity);
			bullet.SetDamageMultiplier(weaponData.damageMultiplier);

            fireRateTimer = FireCooldown;

			currentAmmo--;
			if(currentAmmo <= 0)
			{
				Reload();
			}
		}
	}

	private void Reload()
	{
		reloadTimer = weaponData.reloadTime;
	}

	public void SetShipVelocity(Vector2 velocity)
	{
		shipVelocity = velocity;
	}

	private void OnDrawGizmosSelected()
	{
#if UNITY_EDITOR
		for (int i = 0; i < 3; i++)
		{
			Vector3 worldPos = transform.TransformPoint(FirepointLocalPosition);

			Vector3 direction = Quaternion.Euler(0, 0, FirepointRotation) * Vector3.up;

			DrawArrow.ForGizmo(worldPos, direction, gizmoColor, 5, 20, arrowLength);
		}
#endif
	}
}
