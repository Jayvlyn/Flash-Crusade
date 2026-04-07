using UnityEngine;
using UnityEngine.UIElements;

public class Ship : SpaceObject
{
    public SpriteRenderer sprite;

    public int maxEnergy;
    public int mobility;
    public int handling;

    public float energy;

    public float turboModifier = 1.5f;


    public float redirect = 10;

    public bool turboActive;
    public bool turnRight;
    public bool turnLeft;
    bool braking;


    public AnimationCurve redirectVelocityCurve;
    public AnimationCurve redirectAngularVelocityCurve;

    public void Thrust(Vector2 dir)
    {
        dir = dir.normalized;

        Vector2 worldDir = transform.up * dir.y + transform.right * dir.x;

        float force = mobility;

        if (Velocity.sqrMagnitude > 0.001f)
        {
            Vector2 velocityDirection = Velocity.normalized;

            Vector2 forward = transform.up;
            Vector2 right = transform.right;
            Vector2 frontFacing = forward;

            if (dir.y != 0) // forward/backwards input
                frontFacing = forward;
            else if (dir.x != 0) // side input
                frontFacing = right;

            float alignment = Vector2.Dot(velocityDirection, frontFacing);
            float misalignment = 1f - Mathf.Clamp01(alignment);

            float normalizedVelocity = Mathf.Clamp01(Velocity.sqrMagnitude / (MaxVelocity * MaxVelocity));
            float normalizedAngVelocity = Mathf.Clamp01(Mathf.Abs(AngularVelocity) / MaxAngularVelocity);

            float evaluatedVel = redirectVelocityCurve.Evaluate(normalizedVelocity);
            float evaluatedAngVel = redirectAngularVelocityCurve.Evaluate(normalizedAngVelocity);
            
            float calculation = 1f + misalignment * evaluatedVel * (evaluatedAngVel * redirect);

            force *= calculation;
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
