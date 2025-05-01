using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;

public abstract class AIAgent : MonoBehaviour
{
	[SerializeField] private Ship ship;
    public Ship Ship { get { return ship; } 
        set { ship = value; }
    }

	[SerializeField] private float tickInterval = 0.5f;
    private float tickTimer = 0;

    [SerializeField] private Transform currentTarget;
    private Vector2 moveTarget;
    public Vector2 MoveTarget { 
        get { return moveTarget; } 
        set { moveTarget = value; } 
    }

    [SerializeField] private Ship leader;
    public Ship Leader { get; set; }

    private bool freeFly;
    public bool FreeFly{ get; set; }

    protected virtual void Update()
    {
        if (!freeFly)
        {
            MoveToTarget(moveTarget);
        }
        if (tickTimer <= 0)
        {
            Tick();
            tickTimer = tickInterval;
        }
        else
        {
            tickTimer -= Time.deltaTime;
        }
    }

    protected virtual void Tick()
    {
	}

    public float deadzoneRadius = 1f; // Adjust this value to control the deadzone size

    protected virtual void MoveToTarget(Vector2 target)
    {
        Vector2 targetVelocity = leader.Velocity;
        Vector2 relativeVelocity = ship.Velocity - targetVelocity;
        Vector2 toTarget = target - (Vector2)transform.position;
        float distance = toTarget.magnitude;
        Vector2 direction = toTarget.normalized;

        // Transform the direction to local space based on the ship's rotation
        Vector2 localDirection = transform.InverseTransformDirection(direction);

        float approachSpeed = Mathf.Abs(Vector2.Dot(relativeVelocity, direction));

        float estimatedTimeToReach = distance / approachSpeed;

        float relativeSpeed = relativeVelocity.magnitude;
        float maxDeceleration = -ship.maxAcceleration;

        float stoppingDistance = (relativeSpeed * relativeSpeed) / (2 * Mathf.Abs(maxDeceleration)); // d = s^2 / 2a

        // Check if we're far enough to apply thrust
        if (distance > stoppingDistance)
        {
            if (distance > deadzoneRadius)
            {
                Debug.Log("thrust");
                ship.InputData.thrustInput = localDirection;
            }
            else
            {
                ship.InputData.thrustInput = Vector2.zero;
            }
            // Use the local direction for thrust input, which respects ship's rotation
        }
        else
        {
            // Check if the ship is moving towards the target using the dot product
            float dotProduct = Vector2.Dot(ship.Velocity, direction);

            // If dotProduct is positive, it means the ship is still moving towards the target
            if (dotProduct > 0)
            {
                // The ship is moving towards the target, so continue applying reverse thrust
                Debug.Log("reverse thrust");
                ship.InputData.thrustInput = -localDirection;
            }
            else
            {
                // The ship has passed the target or is moving away from it
                ship.InputData.thrustInput = Vector2.zero;
            }
        }
    }


    //protected virtual void MoveToTarget(Vector2 target)
    //{
    //    Vector2 position = transform.position;
    //    Vector2 toTarget = target - position;
    //    float distance = toTarget.magnitude;

    //    Vector2 velocity = ship.Velocity;
    //    Vector2 dirToTarget = toTarget.normalized;

    //    float speedTowardTarget = Vector2.Dot(velocity, dirToTarget);
    //    float stopDuration = ship.stopDuration;

    //    // Stopping distance = projected speed * stopDuration / ln(100)
    //    float stoppingDistance = Mathf.Abs(speedTowardTarget) * stopDuration / Mathf.Log(100);

    //    if (distance <= stoppingDistance)
    //    {
    //        ship.InputData.thrustInput = Vector2.zero;
    //    }
    //    else
    //    {
    //        ship.InputData.thrustInput = new Vector2(
    //            Mathf.Sign(toTarget.x),
    //            Mathf.Sign(toTarget.y)
    //        );
    //    }
    //}
}
