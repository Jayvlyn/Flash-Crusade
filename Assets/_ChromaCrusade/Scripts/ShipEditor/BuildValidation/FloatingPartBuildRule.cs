using UnityEngine;

public class FloatingPartBuildRule : BuildRule
{
    public FloatingPartBuildRule(BuildArea buildArea)
    {
        this.buildArea = buildArea;
    }

    public override bool CheckRule()
    {
        throw new System.NotImplementedException();
    }
}
