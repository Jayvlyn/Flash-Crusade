using System;
using UnityEngine;
using UnityEngine.Events;

public class HelpInputEventListener : MonoBehaviour
{
    [SerializeField] UnityEvent response;

    private void OnEnable()
    {
        EventBus.Subscribe<HelpInputEvent>(OnHelpInput);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<HelpInputEvent>(OnHelpInput);
    }

    public void OnHelpInput(HelpInputEvent e)
    {
        response.Invoke();
    }
}
