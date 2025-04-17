using UnityEngine;

public class Weapon : MonoBehaviour
{
	[Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;
	[SerializeField] private Vector3 firepoint;

	[Header("Arrow Gizmo")]
	[SerializeField] private Color gizmoColor = Color.cyan;
	[SerializeField] private float arrowLength = 0.5f;

	private float FireCooldown { get { return 1 / weaponData.fireRate; } }
	private Vector3 FirepointLocalPosition {  get { return new Vector3(firepoint.x, firepoint.y, 0f); } }
	private float FirepointRotation { get { return transform.eulerAngles.z + firepoint.z; } }
	private float fireRateTimer;

	public void Fire()
	{
		if(fireRateTimer <= 0)
		{
            Vector3 worldPos = transform.TransformPoint(FirepointLocalPosition);

            Quaternion rot = Quaternion.Euler(0, 0, FirepointRotation);

            Bullet bullet = Instantiate(weaponData.bullet, worldPos, rot);
			bullet.SetInitialVelocity(bullet.transform.up * weaponData.muzzleVelocity);
			bullet.SetDamageMultiplier(weaponData.damageMultiplier);

            fireRateTimer = FireCooldown;
		}
	}

	private void Update()
	{
		if (fireRateTimer > 0) fireRateTimer -= Time.deltaTime;
	}

	private void OnDrawGizmosSelected()
	{
#if UNITY_EDITOR
		for (int i = 0; i < 3; i++)
		{
			Vector3 worldPos = transform.TransformPoint(FirepointLocalPosition);

			Vector3 direction = Quaternion.Euler(0, 0, FirepointRotation) * Vector3.up;

			DrawArrow.ForGizmo(worldPos, direction, gizmoColor, 0.25f, 20, arrowLength);
		}
#endif
	}
}
