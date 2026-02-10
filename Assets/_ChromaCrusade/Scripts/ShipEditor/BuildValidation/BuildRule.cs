using UnityEngine;

public abstract class BuildRule
{
    public BuildArea buildArea;

    public abstract bool CheckRule();
}
