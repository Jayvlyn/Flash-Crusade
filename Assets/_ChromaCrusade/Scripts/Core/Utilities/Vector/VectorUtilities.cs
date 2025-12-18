using UnityEngine;

public static class VectorUtilities
{
    public static bool IsDiagonal(Vector2 v)
    {
        return Mathf.Abs(v.x) > 0.1f && Mathf.Abs(v.y) > 0.1f;
    }

    public static bool IsCardinal(Vector2 v)
    {
        return Mathf.Abs(v.x) > 0.1f ^ Mathf.Abs(v.y) > 0.1f;
    }

    public static bool IsNeutral(Vector2 v)
    {
        return v.sqrMagnitude < 0.01f;
    }
}
