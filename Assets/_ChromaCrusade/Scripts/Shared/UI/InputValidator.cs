using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputValidator : MonoBehaviour
{
    [SerializeField] protected TMP_InputField input;
    [SerializeField] protected Image inputFieldBackground;

    void Awake()
    {
        input.onValueChanged.AddListener(OnValueChanged);
    }

    public virtual bool IsValid()
    {
        string text = input.text;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Windows / cross-platform invalid filename chars (/ \ : * ? " < > |)
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

        if (text.IndexOfAny(invalidChars) >= 0)
            return false;

        // prevents input that is only dots or spaces
        if (text.Trim('.', ' ').Length == 0)
            return false;

        return true;
    }

    protected void OnValueChanged(string value)
    {
        if (IsValid())
            inputFieldBackground.color = Assets.Instance.uiGreen;
        else
            inputFieldBackground.color = Assets.Instance.uiRed;
    }

    public void SetText(string newText)
    {
        input.text = newText;
        OnValueChanged(newText);
    }

    public string GetText()
    {
        return input.text;
    }
}
