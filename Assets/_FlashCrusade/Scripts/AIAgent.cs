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
			MoveToTarget(moveTarget);
		}
	}

    protected virtual void MoveToTarget(Vector2 target)
    {
        Vector2 position = transform.position;
        Vector2 toTarget = target - position;
        float distance = toTarget.magnitude;

        Vector2 velocity = ship.Velocity;
        Vector2 dirToTarget = toTarget.normalized;

        float speedTowardTarget = Vector2.Dot(velocity, dirToTarget);
        float stopDuration = ship.stopDuration;

        // Stopping distance = projected speed * stopDuration / ln(100)
        float stoppingDistance = Mathf.Abs(speedTowardTarget) * stopDuration / Mathf.Log(100);

        if (distance <= stoppingDistance)
        {
            ship.InputData.thrustInput = Vector2.zero;
        }
        else
        {
            ship.InputData.thrustInput = new Vector2(
                Mathf.Sign(toTarget.x),
                Mathf.Sign(toTarget.y)
            );
        }
    }
}
