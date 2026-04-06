using UnityEngine;

public class PlayerPossessor : Possessor
{
    public Pilot pilot;

    private void OnEnable()
    {
        EventBus.Subscribe<ThrustInputEvent>(OnThrustInput);
        EventBus.Subscribe<TurnLeftInputEvent>(OnTurnLeftInput);
        EventBus.Subscribe<TurnRightInputEvent>(OnTurnRightInput);
        EventBus.Subscribe<TurboInputEvent>(OnTurboInput);
        EventBus.Subscribe<BrakeInputEvent>(OnBrakeInput);
        EventBus.Subscribe<FireInputEvent>(OnFireInput);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ThrustInputEvent>(OnThrustInput);
        EventBus.Unsubscribe<TurnLeftInputEvent>(OnTurnLeftInput);
        EventBus.Unsubscribe<TurnRightInputEvent>(OnTurnRightInput);
        EventBus.Unsubscribe<TurboInputEvent>(OnTurboInput);
        EventBus.Unsubscribe<BrakeInputEvent>(OnBrakeInput);
        EventBus.Unsubscribe<FireInputEvent>(OnFireInput);
    }

    void OnThrustInput(ThrustInputEvent e) => pilot.commands.thrust = e.input;

    void OnTurnLeftInput(TurnLeftInputEvent e) => pilot.commands.turnLeft = e.input;

    void OnTurnRightInput(TurnRightInputEvent e) => pilot.commands.turnRight = e.input;

    void OnTurboInput(TurboInputEvent e) => pilot.commands.turbo = e.input;

    void OnBrakeInput(BrakeInputEvent e) => pilot.commands.brake = e.input;

    void OnFireInput(FireInputEvent e) => pilot.commands.fire = e.input;
}
