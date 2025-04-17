using UnityEngine;

public class Enemy : AIAgent
{
    [SerializeField] private Ship ship;

    protected override void Update()
    {
        base.Update();
    }

    protected override void Tick()
    {
        base.Tick();

        ship.inputData.thrustInput = new Vector2(0, 1);
        ship.inputData.holdingFireWeapon1 = true;
        ship.inputData.holdingFireWeapon2 = true;
    }
}
