using UnityEngine;

public class Pilot : MonoBehaviour
{
    public string pilotName;

    public Ship controlledShip;
    public Possessor possessor;

    public PilotCommands commands = new();

    public void ProcessCommands()
    {
        if (commands.thrust != Vector2.zero)
            controlledShip.Thrust(commands.thrust);


        controlledShip.TurnLeft(commands.turnLeft);
        controlledShip.TurnRight(commands.turnRight);
        controlledShip.HandleTurning();

        if (commands.brake)
            controlledShip.StartBrake();
        else
            controlledShip.StopBrake();

        controlledShip.ToggleTurbo(commands.turbo);
    }

    private void Update()
    {
        ProcessCommands();
    }
}
