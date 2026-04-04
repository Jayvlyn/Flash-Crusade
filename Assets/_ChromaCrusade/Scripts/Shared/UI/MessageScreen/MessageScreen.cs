using System;
using TMPro;
using UnityEngine;

public class MessageScreen : MonoBehaviour
{
    [SerializeField] GameObject window;
    [SerializeField] TMP_Text message;
    [SerializeField] NavItem okayButton;
    public Action action;

    void OnEnable()
    {
        EventBus.Subscribe<OpenMessageScreenEvent>(OnOpenEvent);
        EventBus.Subscribe<CloseMessageScreenEvent>(OnCloseEvent);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<OpenMessageScreenEvent>(OnOpenEvent);
        EventBus.Unsubscribe<CloseMessageScreenEvent>(OnCloseEvent);
    }

    void OnCloseEvent(CloseMessageScreenEvent e) => Close();
    void OnOpenEvent(OpenMessageScreenEvent e) => Open(e.message, e.action);

    void Open(string message, Action action = null)
    {
        NavState.PrevScreenItem = NavState.HoveredItem;
        this.message.text = message;
        this.action = action;
        window.SetActive(true);
        NavState.inPopupScreen = true;
        EventBus.Publish(new ItemNavEvent { target = okayButton });
    }

    void Close()
    {
        NavItem navTarget = NavState.PrevScreenItem;
        EventBus.Publish(new ItemNavEvent { target = navTarget });
        message.text = string.Empty;
        window.SetActive(false);
        NavState.inPopupScreen = false;
    }

    public void OnOkay()
    {
        action?.Invoke();
        Close();
    }
}
