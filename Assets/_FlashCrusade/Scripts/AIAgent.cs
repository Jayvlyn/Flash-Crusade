using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;

public abstract class AIAgent : MonoBehaviour
{
	[SerializeField] private Ship ship;
    public Ship Ship 
    { 
        get { return ship; } 
        set { ship = value; }
    }

	[SerializeField] private float tickInterval = 0.5f;
    private float tickTimer = 0;

    private Vector2 moveTarget;
    public Vector2 MoveTarget 
    { 
        get { return moveTarget; } 
        set { moveTarget = value; } 
    }

    [SerializeField] private Ship leader;
    public Ship Leader { get; set; }

    public StateMachine sm;
    public FreeFlyState freeFlyState => new FreeFlyState(this);
    public FollowState followState => new FollowState(this);

    private void Awake()
    {
        sm = new StateMachine();
        sm.ChangeState(followState);
    }

    protected virtual void Update()
    {
        sm.Update();
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

    public virtual void MoveToTarget(Vector2 target)
    {
        Vector2 targetVelocity;
        if (leader != null)
        {
            targetVelocity = leader.Velocity;
        }
        else
        {
            targetVelocity = Vector2.zero;
        }
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
        }
        else
        {
            float dotProduct = Vector2.Dot(ship.Velocity, direction);

            if (dotProduct > 0)
            {
                Debug.Log("reverse thrust");
                ship.InputData.thrustInput = -localDirection;
            }
            else
            {
                ship.InputData.thrustInput = Vector2.zero;
            }
        }
    }
}

public class FreeFlyState : IState
{
    AIAgent agent;

    public FreeFlyState(AIAgent agent)
    {
        this.agent = agent;
    }

    public void OnEnter()
    {
        agent.Ship.InputData.thrustInput = Vector2.zero;
    }

    public void OnExit()
    {
    }

    public void OnUpdate()
    {
    }
}

public class FollowState : IState
{
    AIAgent agent;

    public FollowState(AIAgent agent)
    {
        this.agent = agent;
    }


    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void OnUpdate()
    {
        agent.MoveToTarget(agent.MoveTarget);
    }
}
