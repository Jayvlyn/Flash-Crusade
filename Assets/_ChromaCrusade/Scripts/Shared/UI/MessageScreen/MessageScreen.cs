using TMPro;
using UnityEngine;

public class MessageScreen : MonoBehaviour
{
    [SerializeField] GameObject window;
    [SerializeField] TMP_Text message;
    [SerializeField] NavItem okayButton;

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
    void OnOpenEvent(OpenMessageScreenEvent e) => Open(e.message);

    void Open(string message)
    {
        NavState.ItemBeforePopupScreen = NavState.HoveredItem;
        this.message.text = message;
        window.SetActive(true);
        NavState.inPopupScreen = true;
        EventBus.Publish(new ItemNavEvent { target = okayButton });
    }

    void Close()
    {
        NavItem navTarget = NavState.ItemBeforePopupScreen;
        EventBus.Publish(new ItemNavEvent { target = navTarget });
        message.text = string.Empty;
        window.SetActive(false);
        NavState.inPopupScreen = false;
    }

    public void OnOkay()
    {
        Close();
    }
}
