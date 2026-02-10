using UnityEngine;

public class PartTypeBuildRule : BuildRule
{
    public PartType requiredType;
    public int requiredAmount;

    public PartTypeBuildRule(BuildArea buildArea, PartType requiredType, int requiredAmount)
    {
        this.buildArea = buildArea;
        this.requiredType = requiredType;
        this.requiredAmount = requiredAmount;
    }

    public override bool CheckRule()
    {
        throw new System.NotImplementedException();
    }
}
