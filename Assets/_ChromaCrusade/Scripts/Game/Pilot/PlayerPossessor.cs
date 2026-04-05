using UnityEngine;

public class PlayerPossessor : Possessor
{
    public Pilot pilot;

    private void OnEnable()
    {
        EventBus.Subscribe<ThrustInputEvent>(OnThrustInput);
        EventBus.Subscribe<TurnInputEvent>(OnTurnInput);
        EventBus.Subscribe<TurboInputEvent>(OnTurboInput);
        EventBus.Subscribe<BrakeInputEvent>(OnBrakeInput);
        EventBus.Subscribe<FireInputEvent>(OnFireInput);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ThrustInputEvent>(OnThrustInput);
        EventBus.Unsubscribe<TurnInputEvent>(OnTurnInput);
        EventBus.Unsubscribe<TurboInputEvent>(OnTurboInput);
        EventBus.Unsubscribe<BrakeInputEvent>(OnBrakeInput);
        EventBus.Unsubscribe<FireInputEvent>(OnFireInput);
    }

    void OnThrustInput(ThrustInputEvent e)
    {
        //Debug.Log(e.input);
        pilot.commands.thrust = e.input;
    }

    void OnTurnInput(TurnInputEvent e) => pilot.commands.steering = e.input;

    void OnTurboInput(TurboInputEvent e) => pilot.commands.turbo = e.input;

    void OnBrakeInput(BrakeInputEvent e) => pilot.commands.brake = e.input;

    void OnFireInput(FireInputEvent e) => pilot.commands.fire = e.input;
}
