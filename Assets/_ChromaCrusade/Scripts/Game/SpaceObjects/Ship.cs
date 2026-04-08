using UnityEngine;
using UnityEngine.UIElements;

public class Ship : SpaceObject
{

    public SpriteRenderer sprite;

    public int maxEnergy;
    public int mobility;
    public int handling;

    public float energy;

    public float turboModifier = 3f;

    public float redirect = 10;

    public bool turboActive;
    public bool turnRight;
    public bool turnLeft;
    bool braking;

    float turboMaxSpeed;
    float regularMaxSpeed;

    public AnimationCurve redirectVelocityCurve;
    public AnimationCurve redirectAngularVelocityCurve;
    public bool goodMovement = true;

    private void Start()
    {
        regularMaxSpeed = MaxVelocity;
        turboMaxSpeed = MaxVelocity * turboModifier;
    }

    public void Thrust(Vector2 dir)
    {
        if (turboActive) MaxVelocity = turboMaxSpeed;
        else MaxVelocity = regularMaxSpeed;

        dir = dir.normalized;

        Vector2 worldDir = transform.up * dir.y + transform.right * dir.x;

        float force = mobility;

        if (Velocity.sqrMagnitude > 0.001f && goodMovement)
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

    public void TurnRight(bool turnRight)
    {
        this.turnRight = turnRight;
    }

    public void TurnLeft(bool turnLeft)
    {
        this.turnLeft = turnLeft;
    }

    public void HandleTurning()
    {
        if(turnLeft && turnRight)
        {
            AngularDrag = 2;
        }
        else if (turnLeft)
        {
            AngularDrag = 0;
            Turn(-1);
        }
        else if (turnRight)
        {
            AngularDrag = 0;
            Turn(1);
        }
        else if (!braking)
        {
            AngularDrag = 0;
        }
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
            //Debug.Log($"t {t}");

            float curve = t * t;

            float boost = Mathf.Lerp(1f, 30f, curve);
            //Debug.Log($"Boost {boost}");

            force *= boost;
        }

        AddTorque(dir * force);
    }

    public void StartBrake()
    {
        if (braking) return;
        braking = true;
        Drag = 2; // incorp mass later
        AngularDrag = 2;
    }

    public void StopBrake()
    {
        if (!braking) return;
        braking = false;
        Drag = 0;
        AngularDrag = 0;
    }
}
