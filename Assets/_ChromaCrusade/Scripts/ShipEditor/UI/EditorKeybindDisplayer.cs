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

        sb.Append("<color=#272727>");
        sb.Append("Move: ");
        sb.Append(BindingDisplayer.GetKeyboardCompositeBinding(inputManager.NavigateAction.action));
        sb.Append("</color>");

        sb.Append("\n");

        
        sb.Append("<color=#2F2F2F>");
        sb.Append("Select: ");
        sb.Append(BindingDisplayer.GetFirstKeyboardBinding(inputManager.SubmitAction.action));
        sb.Append("</color>");

        sb.Append("\n");

        sb.Append("<color=#272727>");
        sb.Append("Back: ");
        sb.Append(BindingDisplayer.GetKeyboardBindingFromAny(inputManager.CancelAction.action));
        sb.Append("</color>");

        sb.Append("\n");

        sb.Append("<color=#2F2F2F>");
        sb.Append("Change Mode: ");
        sb.Append(BindingDisplayer.GetFirstKeyboardBinding(inputManager.ModeAction.action));
        sb.Append("</color>");

        sb.Append("\n");

        sb.Append("<color=#272727>");
        sb.Append("Next Tab: ");
        sb.Append(BindingDisplayer.GetFirstKeyboardBinding(inputManager.TabAction.action));
        sb.Append("</color>");

        sb.Append("\n");

        sb.Append("<color=#2F2F2F>");
        sb.Append("Zoom: ");
        sb.Append(BindingDisplayer.GetKeyboardZoomBinding(inputManager.ZoomAction.action));
        sb.Append("</color>");




        sb.Append("\n\n");

        sb.Append("<color=#272727>");
        sb.Append("Rotate: ");
        sb.Append(BindingDisplayer.GetKeyboard1DAxisBinding(inputManager.RotateAction.action));
        sb.Append("</color>");

        sb.Append("\n");

        sb.Append("<color=#2F2F2F>");
        sb.Append("Flip: ");
        sb.Append(BindingDisplayer.GetKeyboard1DAxisBinding(inputManager.FlipAction.action));
        sb.Append("</color>");

        sb.Append("\n");

        sb.Append("<color=#272727>");
        sb.Append("Delete: ");
        sb.Append(BindingDisplayer.GetFirstKeyboardBinding(inputManager.DeleteAction.action));
        sb.Append("</color>");



        sb.Append("\n\n");

        sb.Append("<color=#2F2F2F>");
        sb.Append("Undo: ");
        sb.Append(BindingDisplayer.GetModifiedKeyboardBinding(inputManager.UndoAction.action));
        sb.Append("</color>");

        sb.Append("\n");

        sb.Append("<color=#272727>");
        sb.Append("Redo: ");
        sb.Append(BindingDisplayer.GetFirstKeyboardBinding(inputManager.ModifyAction.action));
        sb.Append("+");
        sb.Append(BindingDisplayer.GetModifiedKeyboardBinding(inputManager.UndoAction.action));
        sb.Append("</color>");

        bindingText.text = sb.ToString();
    }
}
