using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipNameValidator : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Image image;

    void Awake()
    {
        input.onValueChanged.AddListener(OnValueChanged); 
    }

    public bool IsValid()
    {
        string text = input.text;

        return !string.IsNullOrEmpty(text);
    }

    void OnValueChanged(string value)
    {
        if (IsValid())
            image.color = Assets.i.uiGreen;
        else
            image.color = Assets.i.uiRed;
    }
}
