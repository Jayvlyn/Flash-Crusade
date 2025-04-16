using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField, Tooltip("Damage dealt to a ship hit by this bullet.")]
    private float damage = 10;

    [SerializeField, Tooltip("How many ships this bullet can pass through before being destroyed."),Range(0,20)]
    private int piercing = 0;

    [SerializeField, Tooltip("How long the bullet will stay airborne before being destroyed.")]
    private float lifetime = 20;

    [SerializeField, Tooltip("How fast bullets turn and move towards targets (0 = No homing)"), Range(0, 30)]
    private float homing = 0;

	private Vector2 velocity;

	private void Update()
	{
		
	}

	private void Move()
	{
		
	}
}
