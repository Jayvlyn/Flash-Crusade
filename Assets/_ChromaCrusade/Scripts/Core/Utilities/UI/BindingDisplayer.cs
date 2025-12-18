using System.Text;
using UnityEngine.InputSystem;

public static class BindingDisplayer
{
    public static string GetAllKeyboardBindings(InputAction action)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (!binding.groups.Contains("Keyboard"))
                continue;

            if (binding.isPartOfComposite)
                continue;

            string display = action.GetBindingDisplayString(i);

            if (sb.Length > 0)
                sb.Append(" / ");

            sb.Append(display);
        }

        return sb.ToString();
    }

    public static string GetKeyboardCompositeBinding(InputAction action)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (!binding.groups.Contains("Keyboard"))
                continue;

            if (binding.isComposite)
                continue;

            string path = binding.effectivePath;
            if (string.IsNullOrEmpty(path))
                continue;

            string key = InputControlPath.ToHumanReadableString(
                path,
                InputControlPath.HumanReadableStringOptions.OmitDevice |
                InputControlPath.HumanReadableStringOptions.UseShortNames
            );

            if (key.Length > 1) 
                continue;

            if (sb.Length > 0)
                sb.Append(" / ");

            sb.Append(key);
        }

        return sb.ToString();
    }

    //public static string GetKeyboard1DAxisBinding(InputAction action)
    //{
        
    //}

    //public static string GetFirstKeyboardBinding(InputAction action)
    //{
    //    var sb = new StringBuilder();

    //    for (int i = 0; i < action.bindings.Count; i++)
    //    {
    //        var binding = action.bindings[i];

    //        if (!binding.groups.Contains("Keyboard"))
    //            continue;

    //        // Skip composite parts (W/A/S/D individually)
    //        if (binding.isPartOfComposite)
    //            continue;

    //        string display = action.GetBindingDisplayString(i);

    //        if (sb.Length > 0)
    //            sb.Append(" / ");

    //        sb.Append(display);
    //    }

    //    return sb.ToString();
    //}
}
