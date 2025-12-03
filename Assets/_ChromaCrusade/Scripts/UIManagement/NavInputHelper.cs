using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavInputHelper : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private TMP_InputField input;

    private void Awake()
    {
        input = GetComponent<TMP_InputField>();
    }

    private void Start()
    {
        input.onSubmit.AddListener(ForceDeselect);
    }

    private void ForceDeselect(string text)
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ActivateInputField()
    {
        if(!input.isFocused)
            input.ActivateInputField();
    }

    public void OnSelect(BaseEventData eventData)
    {
        EventBus.Publish(new NavManager.DisableNavigationEvent());
        EventBus.Publish(new NavManager.EnterInputFieldEvent());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        EventBus.Publish(new NavManager.EnableNavigationEvent());
    }

    private void OnDestroy()
    {
        input.onSubmit.RemoveListener(ForceDeselect);
    }
}
