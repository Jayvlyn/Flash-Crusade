using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class NavItem : MonoBehaviour, IPointerDownHandler
{
    public bool visualize = true;
    public NavItem navUp;
    public NavItem navLeft;
    public NavItem navRight;
    public NavItem navDown;

    public UnityEvent onHighlighted;
    public UnityEvent onSelected;

    private bool disabled;
    public bool Disabled
    {
        get {  return disabled; }
        set {
            disabled = value; 
            if(image != null)
            {
                float alpha = disabled ? 0.1f : 0.5f;
                image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
                if(text != null)
                {
                    alpha = disabled ? 0.1f : 1f;
                    text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                }
            }
        }
    }

    [HideInInspector] public RectTransform rect;
    [HideInInspector] public Image image;
    [HideInInspector] public TMP_Text text;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        text = GetComponentInChildren<TMP_Text>();
    }

    public virtual void OnHighlighted()
    {
        if (Disabled) return;
        onHighlighted?.Invoke();
    }

    public virtual void OnSelected()
    {
        if (Disabled) return;
        onSelected?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnSelected();
    }
}
