using Unity.VisualScripting;
using UnityEngine;

public interface IState
{
    void OnUpdate();
    void OnEnter();
    void OnExit();
}

public class StateMachine
{
    private IState currentState;
    public IState CurrentState => currentState;

    public void ChangeState(IState newState)
    {
        if (currentState == newState) return;

        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();
    }

    public void Update()
    {
        currentState?.OnUpdate();
    }
}
