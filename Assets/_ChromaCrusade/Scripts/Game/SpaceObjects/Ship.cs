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

        AddForce(transform.up * dir * mobility);
    }

    public void Turn(float dir)
    {
        AddTorque(dir * handling);
    }

    public void StartBrake()
    {
        if (Drag != 0) return;
        Drag = 10; // incorp mass later
    }

    public void StopBrake()
    {
        Drag = 0;
    }
}
