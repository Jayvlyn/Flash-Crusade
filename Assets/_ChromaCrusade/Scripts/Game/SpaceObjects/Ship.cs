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
    public float counterSteerPower = 5;

    public bool turboActive;
    public bool turnRight;
    public bool turnLeft;
    bool braking;

    public void Thrust(Vector2 dir)
    {
        dir = dir.normalized;

        Vector2 worldDir = transform.up * dir.y + transform.right * dir.x;

        float forceMod = mobility;

        if (Velocity.sqrMagnitude > 0.001f)
        {
            Vector2 velDir = Velocity.normalized;
            Vector2 forward = transform.up;

            float alignment = Vector2.Dot(velDir, forward);

            float misalignment = 1f - Mathf.Clamp01(alignment);

            float normalizedSpeed = Mathf.Clamp01(Velocity.sqrMagnitude / (MaxVelocity * MaxVelocity));
            Debug.Log(normalizedSpeed);

            forceMod *= 1f + misalignment * (Mathf.Abs(AngularVelocity) * normalizedSpeed);
        }

        AddForce(worldDir * forceMod);
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
