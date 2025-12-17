using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class SpaceObject : MonoBehaviour
{
    [SerializeField] private Vector2 velocity;
    public Vector2 Velocity { get; private set; }

    [SerializeField] private float angularVelocity;
    public float AngularVelocity { get; private set; }

    [SerializeField] private float mass = 1;
    public float Mass { get; private set; }

    [SerializeField] private float drag = 0; // space objects wouldnt really have any drag but we can use this for braking or something.
    public float Drag { get; private set; }

    [SerializeField] private float angularDrag = 0;
    public float AngularDrag { get; private set; }


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
        velocity += (force / mass) * Time.fixedDeltaTime;
    }

    public void AddTorque(float torque)
    {
        angularVelocity += (torque / mass) * Time.fixedDeltaTime;
    }

    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(interval, 0.02f); // minimum step for stability
    }

    public void SimulateStep(float dt)
    {
        velocity *= 1f - (drag * dt);
        angularVelocity *= 1f - (angularDrag * dt);

        rb.MovePosition(rb.position + velocity * dt);
        rb.MoveRotation(rb.rotation + angularVelocity * dt);
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
