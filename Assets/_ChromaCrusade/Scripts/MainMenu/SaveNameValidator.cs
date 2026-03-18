public class SaveNameValidator : InputValidator
{
    public override bool IsValid()
    {
        bool valid = base.IsValid();

        if (PlayerSaveManager.SaveNames.Contains(input.text))
            valid = false;

        return valid;
    }
}
