using UnityEngine;

public class ShipBuildValidator
{
    /** Requirements for valid build:
     * Valid name
     * At least one cabin
     * At least one wing
     * At least one core
     * No floating pieces
     */

    BuildRule[] rules;

    public ShipBuildValidator(BuildArea buildArea, ShipNameValidator nameValidator)
    {
        rules = new BuildRule[5];
        rules[0] = new NameBuildRule(nameValidator);
        rules[1] = new PartTypeBuildRule(buildArea, PartType.Cabin);
        rules[2] = new PartTypeBuildRule(buildArea, PartType.Wing);
        rules[3] = new PartTypeBuildRule(buildArea, PartType.Core);
        rules[4] = new FloatingPartBuildRule(buildArea);
    }

    public string ValidateCurrentBuild()
    {
        string lastResult = "Valid";
        foreach(BuildRule rule in rules)
        {
            lastResult = rule.CheckRule();
            if (!lastResult.Equals("Valid"))
                return lastResult;
        }
        return lastResult;
    }
}
