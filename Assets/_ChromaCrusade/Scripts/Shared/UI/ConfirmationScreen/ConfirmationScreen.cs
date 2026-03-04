using System;
using TMPro;
using UnityEngine;

public class ConfirmationScreen : MonoBehaviour
{
    [SerializeField] GameObject window;
    [SerializeField] TMP_Text message;
    [SerializeField] NavItem noButton;
    NavItem lastNavItem;
    Action action;

    void OnEnable()
    {
        EventBus.Subscribe<OpenConfirmScreenEvent>(OnOpenEvent);
        EventBus.Subscribe<CloseConfirmScreenEvent>(OnCloseEvent);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<OpenConfirmScreenEvent>(OnOpenEvent);
        EventBus.Unsubscribe<CloseConfirmScreenEvent>(OnCloseEvent);
    }

    void OnCloseEvent(CloseConfirmScreenEvent e) => Close();
    void OnOpenEvent(OpenConfirmScreenEvent e) => Open(e.message, e.action, e.lastNavItem);

    void Open(string message, Action action, NavItem lastNavItem)
    {
        this.message.text = message;
        this.action = action;
        this.lastNavItem = lastNavItem;
        window.SetActive(true);
        NavState.inConfirmScreen = true;
        EventBus.Publish(new ItemNavEvent { target = noButton });
    }

    void Close()
    {
        EventBus.Publish(new ItemNavEvent { target = lastNavItem });
        action = null;
        message.text = string.Empty;
        window.SetActive(false);
        NavState.inConfirmScreen = false;
    }

    public void OnYes()
    {
        action?.Invoke();
        Close();
    }

    public void OnNo()
    {
        Close();
    }
}
