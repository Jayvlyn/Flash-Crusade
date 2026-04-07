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

        float forceMod = mobility;

        if (Velocity.sqrMagnitude > 0.001f)
        {
            Vector2 velocityDirection = Velocity.normalized;

            Vector2 forward = transform.up;
            Vector2 right = transform.right;

            float alignment = 0;

            if(dir.y != 0) // move forward or backwards
            {
                alignment = Vector2.Dot(velocityDirection, forward);
            }
            else if(dir.x != 0) // sideways
            {
                alignment = Vector2.Dot(velocityDirection, right);
            }

            //float alignment = Vector2.Dot(velocityDirection, forward);



            float misalignment = 1f - Mathf.Clamp01(alignment);

            float normalizedVelocity = Mathf.Clamp01(Velocity.sqrMagnitude / (MaxVelocity * MaxVelocity));
            float normalizedAngVelocity = Mathf.Clamp01(Mathf.Abs(AngularVelocity) / MaxAngularVelocity);

            // try 1
            //forceMod *= 1f + misalignment * (Mathf.Abs(AngularVelocity) * normalizedVelocity);
            // replace above Angvel * normalizedSpeed with evlauated curves, we can find normalized ang velocity too.
            // And then use the normalized values and evaluate them on a custom animation curve

            // try 2
            //forceMod *= 1 + misalignment * (redirect * normalizedVelocity * normalizedAngVelocity);

            // try 3
            //forceMod *= 1 + misalignment * (Velocity.sqrMagnitude * normalizedAngVelocity);

            // try 4
            //forceMod *= 1 + misalignment * (redirect * normalizedVelocity);

            // try 5
            //forceMod *= 1f + misalignment * redirectVelocityCurve.Evaluate(normalizedVelocity) * Mathf.Abs(AngularVelocity); // pretty decent

            // try 6

            float evaluatedVel = redirectVelocityCurve.Evaluate(normalizedVelocity);
            float evaluatedAngVel = redirectAngularVelocityCurve.Evaluate(normalizedAngVelocity);

            //forceMod *= 1f + misalignment * evaluatedVel * (evaluatedAngVel * redirect); // pretty decent
            
            float calculation = 1f + misalignment * evaluatedVel * (evaluatedAngVel * redirect);

            forceMod *= calculation;

            Debug.Log("------");
            Debug.Log($"Misalignment: {misalignment}");
            Debug.Log($"Eval Vel: {evaluatedVel}");
            Debug.Log($"Eval Ang: {evaluatedAngVel}");
            Debug.Log($"Calc: {calculation}");
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
