using UnityEngine;

public static class DrawArrow
{
    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float lineLengthMultiplier = 1)
    {
        Gizmos.DrawLine(pos, pos + direction * lineLengthMultiplier);
        DrawArrowHead(pos, direction * lineLengthMultiplier, arrowHeadLength, arrowHeadAngle, isGizmo: true);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float lineLengthMultiplier = 1)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(pos, pos + direction * lineLengthMultiplier);
        DrawArrowHead(pos, direction * lineLengthMultiplier, arrowHeadLength, arrowHeadAngle, isGizmo: true);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float lineLengthMultiplier = 1)
    {
        Debug.DrawLine(pos, pos + direction * lineLengthMultiplier);
        DrawArrowHead(pos, direction * lineLengthMultiplier, arrowHeadLength, arrowHeadAngle, isGizmo: false);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float lineLengthMultiplier = 1)
    {
        Debug.DrawLine(pos, pos + direction * lineLengthMultiplier, color);
        DrawArrowHead(pos, direction * lineLengthMultiplier, arrowHeadLength, arrowHeadAngle, isGizmo: false, color: color);
    }

    private static void DrawArrowHead(Vector3 pos, Vector3 direction, float arrowHeadLength, float arrowHeadAngle, bool isGizmo, Color? color = null)
    {
        if (direction == Vector3.zero) return;

        // Normalize the direction
        Vector3 normDir = direction.normalized;

        // Base point at the end of the main arrow line
        Vector3 arrowTip = pos + direction;

        // Rotate direction for the left and right wings of the arrowhead in 2D (around Z axis)
        Quaternion rot = Quaternion.Euler(0, 0, arrowHeadAngle);
        Vector3 right = rot * -normDir;
        rot = Quaternion.Euler(0, 0, -arrowHeadAngle);
        Vector3 left = rot * -normDir;

        if (isGizmo)
        {
            Gizmos.DrawLine(arrowTip, arrowTip + right * arrowHeadLength);
            Gizmos.DrawLine(arrowTip, arrowTip + left * arrowHeadLength);
        }
        else
        {
            Color drawColor = color ?? Color.white;
            Debug.DrawLine(arrowTip, arrowTip + right * arrowHeadLength, drawColor);
            Debug.DrawLine(arrowTip, arrowTip + left * arrowHeadLength, drawColor);
        }
    }
}