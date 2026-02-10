public class NameBuildRule : BuildRule
{
    ShipNameValidator nameValidator;

    public NameBuildRule(ShipNameValidator nameValidator)
    {
        this.nameValidator = nameValidator;
    }

    public override bool CheckRule()
    {
        return nameValidator.IsValid();
    }
}
