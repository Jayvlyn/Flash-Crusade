using UnityEngine;

public class ShipBuildValidator
{
    /** Requirements for valid build:
     * At least one cabin
     * At least one wing
     * At least one core
     * No floating pieces
     */

    public BuildRule[] rules;

    public ShipBuildValidator(BuildArea buildArea)
    {
        rules = new BuildRule[4];
        rules[0] = new PartTypeBuildRule(buildArea, PartType.Cabin, 1);
        rules[1] = new PartTypeBuildRule(buildArea, PartType.Wing, 1);
        rules[2] = new PartTypeBuildRule(buildArea, PartType.Core, 1);
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
