using System.Text;
using TMPro;
using UnityEngine;

public class EditorKeybindDisplayer : MonoBehaviour
{
    public TMP_Text bindingText;
    public InputManager inputManager;

    private void Start()
    {
        SetBindingText();
    }

    public void SetBindingText()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("Move: ");
        sb.Append(BindingDisplayer.GetKeyboardCompositeBinding(inputManager.NavigateAction.action));

        sb.Append(" ");

        sb.Append("Select: ");
        sb.Append(BindingDisplayer.GetAllKeyboardBindings(inputManager.SubmitAction.action));

        sb.Append(" ");

        sb.Append("Change Mode: ");
        sb.Append(BindingDisplayer.GetAllKeyboardBindings(inputManager.ModeAction.action));

        sb.Append(" ");

        sb.Append("Rotate: ");
        //sb.Append(BindingDisplayer.GetKeyboard1DAxisBinding(inputManager.RotateAction.action));

        sb.Append(" ");

        sb.Append("Flip: ");
        //sb.Append(BindingDisplayer.GetKeyboard1DAxisBinding(inputManager.FlipAction.action));

        bindingText.text = sb.ToString();
    }
}
