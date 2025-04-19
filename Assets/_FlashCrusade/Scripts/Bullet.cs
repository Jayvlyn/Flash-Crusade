using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField, Tooltip("Damage dealt to a ship hit by this bullet.")]
    private float damage = 10;
    private float damageMult;

    [SerializeField, Tooltip("How many ships this bullet can pass through before being destroyed."),Range(0,20)]
    private int piercing = 0;
    private int piercingLeft;

    [SerializeField, Tooltip("How long the bullet will stay airborne before being destroyed.")]
    private float lifetime = 20;

    [SerializeField, Tooltip("How fast bullets turn and move towards targets (0 = No homing)"), Range(0, 30)]
    private float homing = 0;
    public float Homing { get { return homing; } }

    //[SerializeField, Tooltip("Max speed that the bullet can reach")]
    //private float maxSpeed = 40;

	private Vector2 velocity;

    //public Vector2 hurtbox = new Vector2(1f, 0.2f);
    public Bounds hurtbox;
    public LayerMask enemyLayer;


    private void Start()
    {
        piercingLeft = piercing;
    }

    private void Update()
	{
        Move();//transform.up);

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, hurtbox.size, 0f, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                int finalDamage = Mathf.RoundToInt(damage * damageMult);
                damageable.TakeDamage(finalDamage);
                if (piercingLeft-- < 0)
                {
                    Destroy(gameObject);
                    break; // stop processing if bullet is destroyed
                }
            }
        }
    }

    public void SetInitialVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        this.damageMult = multiplier;
    }

	//private void Move(Vector2 direction)
	private void Move()
	{
        //Vector2 worldDirection = (Vector2)transform.up * direction.y + (Vector2)transform.right * direction.x;
        //worldDirection.Normalize();

        //Vector2 targetVelocity = worldDirection.normalized * maxSpeed;

        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + hurtbox.center, hurtbox.size);
    }
}
