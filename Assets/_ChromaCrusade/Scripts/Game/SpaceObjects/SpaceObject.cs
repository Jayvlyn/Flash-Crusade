using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class SpaceObject : MonoBehaviour
{
    [Header("Space Object Properties")]
    [SerializeField] private Vector2 velocity;
    public Vector2 Velocity
    {
        get => velocity;
        private set => velocity = value;
    }

    [SerializeField] private float angularVelocity;
    public float AngularVelocity
    {
        get => angularVelocity;
        private set => angularVelocity = value;
    }

    [SerializeField] private float mass = 1;
    public float Mass
    {
        get => mass;
        private set => mass = value;
    }

    [SerializeField] private float drag = 0;
    public float Drag
    {
        get => drag;
        set
        {
            value = Mathf.Clamp(value, 0, value);
            drag = value;
        }
    }

    [SerializeField] private float angularDrag = 0;
    public float AngularDrag
    {
        get => angularDrag;
        set
        {
            value = Mathf.Clamp(value, 0, value);
            angularDrag = value;
        }
    }

    [SerializeField] private float maxVelocity = 100f;
    public float MaxVelocity
    {
        get => maxVelocity;
        set => maxVelocity = Mathf.Max(0f, value);
    }

    [SerializeField] private float maxAngularVelocity = 400f;
    public float MaxAngularVelocity
    {
        get => maxAngularVelocity;
        set => maxAngularVelocity = Mathf.Max(0f, value);
    }

    private Rigidbody2D rb;

    // custom fixed update
    private float timeAccumulator = 0;
    [SerializeField] private float updateInterval = 0.02f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void AddForce(Vector2 force)
    {
        velocity += (force / mass) * dt;
    }

    public void AddTorque(float torque)
    {
        angularVelocity += (torque / mass) * dt;
    }

    public void SetUpdateInterval(float interval)
    {
        updateInterval = interval;
    }

    float dt;
    public void SimulateStep(float dt)
    {
        this.dt = dt;
        velocity *= 1f - (drag * dt);

        if(velocity.sqrMagnitude > MaxVelocity * MaxVelocity * 2) // way above, smoothly bring back
            velocity *= 1f - (2 * dt); // quickly drag down to max
        else if (velocity.sqrMagnitude > MaxVelocity * MaxVelocity) // change to use mass for max speed instad of hard coded max speed
            velocity = velocity.normalized * MaxVelocity;
   
        angularVelocity *= 1f - (angularDrag * dt);

        if (Mathf.Abs(angularVelocity) > MaxAngularVelocity)
            angularVelocity = Mathf.Sign(angularVelocity) * MaxAngularVelocity;

        rb.MovePosition(rb.position + velocity * dt);
        rb.MoveRotation(rb.rotation - angularVelocity * dt); // - angular velocity means positive angVel = clockwise rot
    }

    public void Tick(float deltaTime)
    {
        timeAccumulator += deltaTime;
        if (timeAccumulator >= updateInterval)
        {
            SimulateStep(timeAccumulator);
            timeAccumulator = 0f;
        }
    }
}
