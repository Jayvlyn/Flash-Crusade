using UnityEngine;
using UnityEngine.UI;

public class SaveNameValidator : InputValidator
{
    [SerializeField] PlayerSaveManager saveManager;

    public override bool IsValid()
    {
        bool valid = base.IsValid();

        if (saveManager.saveNames.Contains(input.text))
            valid = false;

        return valid;
    }
}
