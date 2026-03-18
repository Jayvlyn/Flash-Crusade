using System;
using TMPro;
using UnityEngine;

public class ConfirmationScreen : MonoBehaviour
{
    [SerializeField] GameObject window;
    [SerializeField] TMP_Text message;
    [SerializeField] NavItem noButton;
    NavItem yesNavItem;
    NavItem noNavItem;
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
    void OnOpenEvent(OpenConfirmScreenEvent e) => Open(e.message, e.action, e.yesNavItem, e.noNavItem);

    void Open(string message, Action action, NavItem yesNavItem, NavItem noNavItem)
    {
        NavState.PrevScreenItem = NavState.HoveredItem;
        this.message.text = message;
        this.action = action;
        this.yesNavItem = yesNavItem;
        this.noNavItem = noNavItem;
        window.SetActive(true);
        NavState.inPopupScreen = true;
        EventBus.Publish(new ItemNavEvent { target = noButton });
    }

    void Close(bool yes = false)
    {
        NavItem navTarget = yes ? yesNavItem : noNavItem;
        if (navTarget == null) navTarget = NavState.PrevScreenItem;
        EventBus.Publish(new ItemNavEvent { target = navTarget });
        action = null;
        message.text = string.Empty;
        window.SetActive(false);
        NavState.inPopupScreen = false;
        yesNavItem = null;
        noNavItem = null;
    }

    public void OnYes()
    {
        action?.Invoke();
        Close(true);
    }

    public void OnNo()
    {
        Close(false);
    }
}
