using UnityEngine;

public class SpaceObject : MonoBehaviour
{
    [SerializeField] private Vector2 velocity;
    public Vector2 Velocity { get; private set; }

    [SerializeField] private Vector2 angularVelocity;
    public Vector2 AngularVelocity { get; private set; }

}
