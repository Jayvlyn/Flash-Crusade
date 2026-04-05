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
        {
            controlledShip.Thrust(commands.thrust);
        }

        if (commands.steering != 0)
        {
            controlledShip.Turn(commands.steering);
        }

        if (commands.brake)
            controlledShip.StartBrake();
        else
            controlledShip.StopBrake();

        //Debug.Log(commands.brake);

        controlledShip.turboActive = commands.turbo;
    }

    private void Update()
    {
        ProcessCommands();
    }
}
