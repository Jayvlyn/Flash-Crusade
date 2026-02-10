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

    public override bool CheckRule()
    {
        return buildArea.HasPartType(requiredType);
    }
}
