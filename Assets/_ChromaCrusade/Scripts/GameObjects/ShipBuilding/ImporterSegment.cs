using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button)), RequireComponent(typeof(Image))]
public class ImporterSegment : MonoBehaviour
{
    public Color DisabledColor = Color.ghostWhite;
    public Color EnabledColor = Color.white;

    public ImporterConnection leftConnection;
    public ImporterConnection topConnection;
    public ImporterConnection rightConnection;
    public ImporterConnection bottomConnection;

    private Button button;
    private Image image;

    private enum SegmentState { Disabled, Enabled }
    private SegmentState segmentState;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        ChangeState(SegmentState.Disabled);
    }

    public void OnClick()
    {
        ToggleState();
    }

    private void ToggleState()
    {
        if (segmentState == SegmentState.Disabled) ChangeState(SegmentState.Enabled);
        else                                       ChangeState(SegmentState.Disabled);
    }

    private void ChangeState(SegmentState state)
    {
        segmentState = state;
        switch (segmentState)
        {
            case SegmentState.Disabled:
                image.color = DisabledColor;
                SetConnectionsActive(false);
                break;
            case SegmentState.Enabled:
                image.color = EnabledColor;
                SetConnectionsActive(true);
                break;
        }
    }

    private void SetConnectionsActive(bool active)
    {
        leftConnection.gameObject.SetActive(active);
        topConnection.gameObject.SetActive(active);
        rightConnection.gameObject.SetActive(active);
        bottomConnection.gameObject.SetActive(active);
    }
}
