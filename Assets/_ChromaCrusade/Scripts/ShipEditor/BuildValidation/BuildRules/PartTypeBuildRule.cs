using UnityEngine;

public class PartTypeBuildRule : BuildRule
{
    BuildArea buildArea;
    PartType requiredType;

    public PartTypeBuildRule(BuildArea buildArea, PartType requiredType)
    {
        this.buildArea = buildArea;
        this.requiredType = requiredType;
    }

    public override string CheckRule()
    {
        if(buildArea.HasPartType(requiredType))
            return passingString;

        return $"Your ship needs at least one {requiredType.ToString()}!";
    }
}
