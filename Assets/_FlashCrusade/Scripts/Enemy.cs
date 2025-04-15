using UnityEngine;

public class Enemy : AIAgent
{
    [SerializeField] private Ship ship;
    private ShipInputData inputData = new ShipInputData();

    protected override void Update()
    {
        base.Update();
    }

    protected override void Tick()
    {
        base.Tick();
    }
}
