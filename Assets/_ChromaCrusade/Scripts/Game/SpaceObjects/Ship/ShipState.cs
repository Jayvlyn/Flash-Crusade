using System;
using UnityEngine;

[Serializable]
public struct ShipState
{
    public bool turboActive;
    public bool turnRight;
    public bool turnLeft;
    public bool braking;
    public float energy;

    public bool TurningOnlyLeft => turnLeft && !turnRight;
    public bool TurningOnlyRight => turnRight && !turnLeft;
    public bool NeutralizingSteer => turnLeft && turnRight;
}
