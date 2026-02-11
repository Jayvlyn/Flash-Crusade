public class NameBuildRule : BuildRule
{
    ShipNameValidator nameValidator;

    public NameBuildRule(ShipNameValidator nameValidator)
    {
        this.nameValidator = nameValidator;
    }

    public override string CheckRule()
    {
        if (nameValidator.IsValid()) 
            return passingString;

        return "You must enter a valid name for your ship!";
    }
}
