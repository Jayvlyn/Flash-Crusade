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

    //[SerializeField, Tooltip("Max speed that the bullet can reach")]
    //private float maxSpeed = 40;

	private Vector2 velocity;

	private void Update()
	{
        Move();//transform.up);
    }

    public void SetInitialVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }

	//private void Move(Vector2 direction)
	private void Move()
	{
        //Vector2 worldDirection = (Vector2)transform.up * direction.y + (Vector2)transform.right * direction.x;
        //worldDirection.Normalize();

        //Vector2 targetVelocity = worldDirection.normalized * maxSpeed;

        transform.position += (Vector3)(velocity * Time.deltaTime);
    }
}
