using UnityEngine;

public abstract class AIAgent : MonoBehaviour
{
    [SerializeField] private float tickInterval = 0.5f;
    private float tickTimer = 0;

    protected virtual void Update()
    {
        if (tickTimer <= 0)
        {
            Tick();
            tickTimer = tickInterval;
        }
        else
        {
            tickTimer -= 0;
        }
    }

    protected virtual void Tick()
    {

    }
}
