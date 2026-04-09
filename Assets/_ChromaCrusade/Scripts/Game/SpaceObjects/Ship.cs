using UnityEngine;

public class Ship : SpaceObject
{
    [Header("Ship Properties")]
    public SpriteRenderer sprite;

    public float brakeDrag = 2;
    public float steerBrakeDrag = 4;

    public int maxEnergy;
    public int mobility;
    public int handling;

    public float energy;

    public float turboModifier = 3f;

    public float redirect = 10;

    bool turboActive;
    bool turnRight;
    bool turnLeft;
    bool braking;

    float turboMaxSpeed;
    float regularMaxSpeed;

    public AnimationCurve redirectVelocityCurve;
    public AnimationCurve redirectAngularVelocityCurve;

    #region Lifecycle

    private void Start()
    {
        regularMaxSpeed = MaxVelocity;
        turboMaxSpeed = MaxVelocity * turboModifier;
    }

    private void Update()
    {
        ProcessDrag();
    }

    #endregion

    public void Thrust(Vector2 dir)
    {
        if (turboActive) MaxVelocity = turboMaxSpeed;
        else MaxVelocity = regularMaxSpeed;

        dir = dir.normalized;

        Vector2 worldDir = transform.up * dir.y + transform.right * dir.x;

        float force = mobility;

        if (Velocity.sqrMagnitude > 0.001f)
        {
            Vector2 velocityDirection = Velocity.normalized;

            Vector2 frontFacing;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                frontFacing = dir.x > 0 ? (Vector2)transform.right : -(Vector2)transform.right;
            else
                frontFacing = dir.y > 0 ? (Vector2)transform.up : -(Vector2)transform.up;

            float alignment = Vector2.Dot(velocityDirection, frontFacing);
            float misalignment = 1f - Mathf.Clamp01(alignment);

            float normalizedVelocity = Mathf.Clamp01(Velocity.sqrMagnitude / (MaxVelocity * MaxVelocity));
            float normalizedAngVelocity = Mathf.Clamp01(Mathf.Abs(AngularVelocity) / MaxAngularVelocity);

            float evaluatedVel = redirectVelocityCurve.Evaluate(normalizedVelocity);
            float evaluatedAngVel = redirectAngularVelocityCurve.Evaluate(normalizedAngVelocity);

            float calculation = 1f + misalignment * evaluatedVel * (evaluatedAngVel * redirect);

            force *= calculation;

            if (turboActive) force *= turboModifier;
        }

        AddForce(worldDir * force);
    }

    public void Turn(float dir)
    {
        float force = handling;

        float angVel = AngularVelocity;

        // Opposing input detection
        float opposition = -Mathf.Sign(angVel) * dir; // +1 = fully opposing, -1 = same direction

        if (opposition > 0f)
        {
            float absVel = Mathf.Abs(angVel);

            float t = Mathf.Clamp01(absVel / MaxAngularVelocity);

            float curve = t * t;

            float boost = Mathf.Lerp(1f, 30f, curve);

            force *= boost; // additional force to help counter steer
        }

        AddTorque(dir * force);
    }

    public void HandleTurning()
    {
        if (turnLeft && !turnRight) Turn(-1);
        else if (turnRight && !turnLeft) Turn(1);
    }

    public void StartBrake()
    {
        braking = true;
    }

    public void StopBrake()
    {
        braking = false;
    }

    public void TurnRight(bool turnRight)
    {
        this.turnRight = turnRight;
    }

    public void TurnLeft(bool turnLeft)
    {
        this.turnLeft = turnLeft;
    }

    public void ToggleTurbo(bool toggle)
    {
        turboActive = toggle;
    }

    public void ProcessDrag()
    {
        if(braking) // braking drag both
        {
            Drag = brakeDrag;
            AngularDrag = steerBrakeDrag;
        }
        else if(turnLeft && turnRight) // not braking but negating steer
        {
            Drag = 0;
            AngularDrag = steerBrakeDrag;
        }
        else // not braking or negating steer
        {
            Drag = 0;
            AngularDrag = 0;
        }
    }
}
