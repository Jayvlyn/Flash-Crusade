public class FloatingPartBuildRule : BuildRule
{
    BuildArea buildArea;

    public FloatingPartBuildRule(BuildArea buildArea)
    {
        this.buildArea = buildArea;
    }

    public override bool CheckRule()
    {
        return !buildArea.HasDisconnectedPart();
    }
}
