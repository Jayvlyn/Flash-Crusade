using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button)), RequireComponent(typeof(Image))]
public class ImporterConnection : MonoBehaviour
{
    public ImporterSegment adjacentSegment;

    public Color BlockedColor = Color.red;
    public Color DisabledColor = Color.yellow;
    public Color EnabledColor = Color.green;

    private Button button;
    private Image image;

    public ConnectionState connectionState;
    private ConnectionState prevState;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    private void Start()
    {
        ChangeState(ConnectionState.Disabled);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<SegmentToggledEvent>(OnSegmentToggled);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SegmentToggledEvent>(OnSegmentToggled);
    }

    private void ChangeState(ConnectionState state)
    {
        if (connectionState == state) return;
        prevState = connectionState;
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

    public void UpdateState()
    {
        if (adjacentSegment == null) return;
        if (adjacentSegment.segmentState == SegmentState.Enabled) ChangeState(ConnectionState.Blocked);
        else if (connectionState == ConnectionState.Blocked) ChangeState(prevState);
    }

    public void OnSegmentToggled(SegmentToggledEvent e)
    {
        UpdateState();
    }

    public void Activate(bool active)
    {
        image.enabled = active;
        button.enabled = active;
        if (active && connectionState == ConnectionState.Enabled) ChangeState(ConnectionState.Disabled);
    }
}
