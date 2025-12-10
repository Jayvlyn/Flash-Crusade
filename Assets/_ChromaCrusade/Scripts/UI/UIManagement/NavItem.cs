using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class NavItem : MonoBehaviour
{
    public NavItem navUp;
    public NavItem navLeft;
    public NavItem navRight;
    public NavItem navDown;

    public UnityEvent onHighlighted;
    public UnityEvent onSelected;

    [HideInInspector] public RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public virtual void OnHighlighted()
    {
        onHighlighted?.Invoke();
    }

    public virtual void OnSelected()
    {
        onSelected?.Invoke();
    }
}
