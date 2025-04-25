using System.IO;
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

    [SerializeField] private Transform leader;
    public Transform Leader { get; set; }

    private bool freeFly;
    public bool FreeFly{ get; set; }

    protected virtual void Update()
    {
        MoveToTarget(moveTarget);
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
		if (!freeFly)
		{
			//MoveToTarget(moveTarget);
		}
	}

	//protected virtual void MoveToTarget(Vector2 target)
	//{
	//	Vector2 position = transform.position;
	//	Vector2 toTarget = target - position;
	//	float distance = toTarget.magnitude;

	//	Vector2 velocity = ship.Velocity;
	//	Vector2 dirToTarget = toTarget.normalized;

	//	float speedTowardTarget = Vector2.Dot(velocity, dirToTarget);
	//	float stopDuration = ship.stopDuration;

	//	// Stopping distance = projected speed * stopDuration / ln(100)
	//	float stoppingDistance = Mathf.Abs(speedTowardTarget) * stopDuration / Mathf.Log(100);

	//	if (distance <= stoppingDistance)
	//	{
	//		ship.InputData.thrustInput = Vector2.zero;
	//	}
	//	else
	//	{
	//		// Convert world direction to local direction
	//		Vector2 localDirToTarget = transform.InverseTransformDirection(dirToTarget);

	//		// Now input is aligned with ship's local forward (y) and right (x)
	//		ship.InputData.thrustInput = new Vector2(
	//			Mathf.Sign(localDirToTarget.x),
	//			Mathf.Sign(localDirToTarget.y)
	//		);
	//	}
	//}

	protected virtual void MoveToTarget(Vector2 target)
	{
		Vector2 position = transform.position;
		Vector2 toTarget = target - position;
		float distance = toTarget.magnitude;

		Vector2 velocity = ship.Velocity;
		Vector2 dirToTarget = toTarget.normalized;

		// Convert world-space direction to local space
		Vector2 localDirToTarget = transform.InverseTransformDirection(dirToTarget);
		Vector2 localVelocity = transform.InverseTransformDirection(velocity);

		// Calculate stopping distance: v^2 / (2a)
		float acceleration = ship.acceleration.magnitude; // You should define this on your ship
		float speedTowardTarget = Vector2.Dot(velocity, dirToTarget);
		float stoppingDistance = (speedTowardTarget * speedTowardTarget) / (2f * acceleration);

		// Decide if we should apply forward or reverse thrust
		if (distance <= stoppingDistance)
		{
			// Apply reverse thrust to stop more quickly
			Vector2 reverseDir = -localVelocity.normalized;
			ship.InputData.thrustInput = new Vector2(
				Mathf.Sign(reverseDir.x),
				Mathf.Sign(reverseDir.y)
			);
		}
		else
		{
			// Apply thrust toward the target
			ship.InputData.thrustInput = new Vector2(
				Mathf.Sign(localDirToTarget.x),
				Mathf.Sign(localDirToTarget.y)
			);
		}
	}

}
