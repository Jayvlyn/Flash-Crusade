public class FloatingPartBuildRule : BuildRule
{
    BuildArea buildArea;

    public FloatingPartBuildRule(BuildArea buildArea)
    {
        this.buildArea = buildArea;
    }

    public override string CheckRule()
    {
        if (buildArea.HasDisconnectedPart())
            return "All ship parts in the build area must be connected! (No floating parts)";
        return passingString;
    }
}
