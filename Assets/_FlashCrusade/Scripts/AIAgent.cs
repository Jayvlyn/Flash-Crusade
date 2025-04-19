using UnityEngine;

public abstract class AIAgent : MonoBehaviour
{
    [SerializeField] private float tickInterval = 0.5f;
    private float tickTimer = 0;

    [SerializeField] private Transform currentTarget;

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

    }
}
