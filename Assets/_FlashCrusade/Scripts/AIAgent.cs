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


        if (!freeFly)
        {
            MoveToTarget(moveTarget);
        }
    }

    protected virtual void Tick()
    {

    }

    protected virtual void MoveToTarget(Vector2 target)
    {

    }
}
