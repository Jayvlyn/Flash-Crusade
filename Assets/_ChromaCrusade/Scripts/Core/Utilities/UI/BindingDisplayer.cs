using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
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

    public static string GetKeyboard1DAxisBinding(InputAction action)
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


    public static string GetKeyboardZoomBinding(InputAction action)
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

            if (key.Equals("=")) 
                key = "+";

            sb.Append(key);
        }

        return sb.ToString();
    }

    public static string GetFirstKeyboardBinding(InputAction action)
    {
        var sb = new StringBuilder();

        var binding = action.bindings[0];

        if (!binding.groups.Contains("Keyboard"))
            return sb.ToString();

        string path = binding.effectivePath;
        if (string.IsNullOrEmpty(path))
            return sb.ToString();

        string key = InputControlPath.ToHumanReadableString(
            path,
            InputControlPath.HumanReadableStringOptions.OmitDevice |
            InputControlPath.HumanReadableStringOptions.UseShortNames
        );

        sb.Append(key);

        return sb.ToString();
    }

    public static string GetKeyboardBindingFromAny(InputAction action)
    {
        foreach (var binding in action.bindings)
        {
            if (binding.isPartOfComposite)
                continue;

            var effectivePath = binding.effectivePath;
            if (string.IsNullOrEmpty(effectivePath))
                continue;

            // TryFindControls resolves the abstract [Any] path to actual controls
            var controls = InputSystem.FindControls(effectivePath)
                                      .Where(c => c.device is Keyboard)
                                      .ToArray();

            if (controls.Length > 0)
            {
                // Pick the first control (usually what you want)
                string key = InputControlPath.ToHumanReadableString(
                    controls[0].path,
                    InputControlPath.HumanReadableStringOptions.OmitDevice |
                    InputControlPath.HumanReadableStringOptions.UseShortNames
                );
                return ShortenKey(CapitalizeFirst(key));
            }
        }

        return string.Empty;
    }

    public static string GetModifiedKeyboardBinding(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var composite = action.bindings[i];

            if (!composite.isComposite)
                continue;

            string modifier = null;
            string key = null;
            bool hasKeyboardPart = false;

            for (int j = i + 1; j < action.bindings.Count; j++)
            {
                var part = action.bindings[j];
                if (!part.isPartOfComposite)
                    break;

                if (part.groups.Contains("Keyboard"))
                    hasKeyboardPart = true;

                var readable = InputControlPath.ToHumanReadableString(
                    part.effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice |
                    InputControlPath.HumanReadableStringOptions.UseShortNames
                );

                if (modifier == null)
                    modifier = readable;
                else
                    key = readable;
            }

            if (!hasKeyboardPart)
                continue;

            if (modifier != null && key != null)
                return $"{ShortenKey(modifier)}+{key}";
        }

        return string.Empty;
    }

    static string ShortenKey(string value)
    {
        return value switch
        {
            "Control" => "Ctrl",
            "Left Control" => "Ctrl",
            "Right Control" => "Ctrl",
            "Shift" => "Shift",
            "Left Shift" => "Shift",
            "Right Shift" => "Shift",
            "Alt" => "Alt",
            "Left Alt" => "Alt",
            "Right Alt" => "Alt",
            "Escape" => "Esc",
            _ => value
        };
    }

    static string CapitalizeFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
