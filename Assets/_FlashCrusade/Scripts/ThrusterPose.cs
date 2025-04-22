using UnityEngine;

[System.Serializable]
public struct ThrusterPose
{
    public Vector2 inactivePos;
    public Vector2 activePos;
    public float zRotation;

    public ThrusterPose(Vector2 inactivePos, Vector2 activePos, float zRotation)
    {
        this.inactivePos = inactivePos;
        this.activePos = activePos;
        this.zRotation = zRotation;
    }
}