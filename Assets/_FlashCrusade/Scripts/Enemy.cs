using UnityEngine;

public class Enemy : AIAgent
{

    protected override void Update()
    {
        base.Update();
        MoveTarget = PlayerController.I.transform.position;
    }

    protected override void Tick()
    {
        base.Tick();
    }
}
