public abstract class BuildRule
{
    protected string passingString = "Valid";

    /// <summary>
    /// Checks if current build abides by this rule
    /// </summary>
    /// <returns>"Valid" if build passes rule, otherwise returns string explaining what rule was broken</returns>
    public abstract string CheckRule();
}
