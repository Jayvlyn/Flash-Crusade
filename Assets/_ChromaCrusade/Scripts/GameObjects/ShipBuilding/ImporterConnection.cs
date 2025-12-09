using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button)), RequireComponent(typeof(Image))]
public class ImporterConnection : MonoBehaviour
{
    public Color BlockedColor = Color.red;
    public Color DisabledColor = Color.yellow;
    public Color EnabledColor = Color.green;

    private Button button;
    private Image image;

    private enum ConnectionState { Blocked, Disabled, Enabled }
    // Blocked = Red, Diabled = Yellow, Enabled = Green
    private ConnectionState connectionState;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        ChangeState(ConnectionState.Disabled);
    }

    private void ChangeState(ConnectionState state)
    {
        connectionState = state;
        switch (connectionState)
        {
            case ConnectionState.Blocked:
                image.color = BlockedColor;
                button.interactable = false;
                break;
            case ConnectionState.Disabled:
                image.color = DisabledColor;
                button.interactable = true;
                break;
            case ConnectionState.Enabled:
                image.color = EnabledColor;
                button.interactable = true;
                break;
        }
    }

    public void Unblock()
    {
        ChangeState(ConnectionState.Disabled);
    }

    public void Block()
    {
        ChangeState(ConnectionState.Blocked);
    }

    public void OnSelected()
    {
        if      (connectionState == ConnectionState.Disabled) ChangeState(ConnectionState.Enabled);
        else if (connectionState == ConnectionState.Enabled)  ChangeState(ConnectionState.Disabled);
    }
}
