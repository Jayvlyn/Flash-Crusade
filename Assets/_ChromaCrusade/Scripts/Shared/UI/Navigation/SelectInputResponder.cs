using UnityEngine;

public class SelectInputResponder : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<SubmitInputEvent>(OnSubmitEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SubmitInputEvent>(OnSubmitEvent);
    }

    void OnSubmitEvent(SubmitInputEvent e)
    {
        if (NavState.HoveredItem != null)
        {
            NavState.HoveredItem.OnSelected();
        }
    }
}
