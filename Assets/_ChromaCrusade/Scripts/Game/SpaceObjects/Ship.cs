using UnityEngine;

public class Ship : SpaceObject
{
    public SpriteRenderer sprite;

    public int maxEnergy;
    public int mobility;
    public int handling;

    public float energy;

    public float turboModifier = 1.5f;
    public bool turboActive;

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

            // solution 1
            float misalignment = 1f - Mathf.Clamp01(alignment);

            //Debug.Log(misalignment);

            forceMod *= 1f + misalignment * Mathf.Abs(AngularVelocity);
        }
        //Debug.Log(forceMod);
        Debug.Log(worldDir);

        AddForce(worldDir * forceMod);
    }

    public void Turn(float dir)
    {
        //turning = true;

        float force = handling;

        float absVel = Mathf.Abs(AngularVelocity);

        if (dir > 0 && AngularVelocity < 0 || dir < 0 && AngularVelocity > 0)
        {
            if(absVel > 1)
                force *= Mathf.Clamp(absVel / 5, 1, absVel);
        }

        //Debug.Log(force);

        AddTorque(dir * force);
    }

    public void StopTurn()
    {
        //turning = false;
    }

    public void StartBrake()
    {
        if (Drag != 0) return;
        Drag = 2; // incorp mass later
        AngularDrag = 2;
    }

    public void StopBrake()
    {
        Drag = 0;
        AngularDrag = 0;
    }
}
