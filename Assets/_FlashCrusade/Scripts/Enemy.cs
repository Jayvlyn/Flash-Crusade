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

  //      ship.inputData.thrustInput = Random.insideUnitCircle;
  //      ship.inputData.holdingFireWeapon1 = Random.Range(0, 2) == 1;
		//ship.inputData.holdingFireWeapon2 = Random.Range(0, 2) == 1;
  //      ship.inputData.turnInput = Random.Range(-1, 2);
    }
}
