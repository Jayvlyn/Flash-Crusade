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

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Windows / cross-platform invalid filename chars (/ \ : * ? " < > |)
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

        if (text.IndexOfAny(invalidChars) >= 0)
            return false;

        // prevent names that are only dots or spaces
        if (text.Trim('.', ' ').Length == 0)
            return false;

        return true;
    }

    public string GetName()
    {
        return input.text;
    }

    public void SetName(string newName)
    {
        input.text = newName;
        OnValueChanged(newName);
    }

    void OnValueChanged(string value)
    {
        if (IsValid())
            image.color = Assets.i.uiGreen;
        else
            image.color = Assets.i.uiRed;
    }
}
