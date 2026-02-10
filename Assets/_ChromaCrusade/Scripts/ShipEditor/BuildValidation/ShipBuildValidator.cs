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

    [SerializeField] ShipNameValidator nameValidator;

    BuildRule[] rules;

    public ShipBuildValidator(BuildArea buildArea)
    {
        rules = new BuildRule[5];
        rules[0] = new NameBuildRule(nameValidator);
        rules[0] = new PartTypeBuildRule(buildArea, PartType.Cabin);
        rules[1] = new PartTypeBuildRule(buildArea, PartType.Wing);
        rules[2] = new PartTypeBuildRule(buildArea, PartType.Core);
        rules[3] = new FloatingPartBuildRule(buildArea);
    }

    public bool ValidateCurrentBuild()
    {
        foreach(BuildRule rule in rules)
        {
            if (!rule.CheckRule()) 
                return false;
        }
        return true;
    }
}
