using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipNameValidator : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Image image;

    private void Awake()
    {
        input.onValueChanged.AddListener(OnValueChanged); 
    }

    void OnValueChanged(string value)
    {
        if (IsValid())
            image.color = Assets.i.uiGreen;
        else
            image.color = Assets.i.uiRed;
    }

    private bool IsValid()
    {
        string text = input.text;

        return !string.IsNullOrEmpty(text);
    }
}
