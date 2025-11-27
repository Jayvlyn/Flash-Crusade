using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Bool3x3))]
public class BoolGrid3x3Drawer : PropertyDrawer
{
    private const int GRID_SIZE = 3;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight * (GRID_SIZE + 1);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw label
        position = EditorGUI.PrefixLabel(position, label);

        float cellSize = 20f;                    // width/height of each toggle
        float spacing = 4f;                     // spacing between cells
        float totalWidth = GRID_SIZE * cellSize + (GRID_SIZE - 1) * spacing;

        // Center the grid horizontally within property rect
        float startX = position.x;
        float startY = position.y;// + EditorGUIUtility.singleLineHeight + 2f;

        DrawGrid(property, startX, startY, cellSize, spacing);
    }

    private void DrawGrid(SerializedProperty property, float startX, float startY, float size, float spacing)
    {
        string[,] names =
        {
            { "r0c0", "r0c1", "r0c2" },
            { "r1c0", "r1c1", "r1c2" },
            { "r2c0", "r2c1", "r2c2" },
        };

        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                var prop = property.FindPropertyRelative(names[y, x]);
                var rect = new Rect(
                    startX + x * (size + spacing),
                    startY + y * (size + spacing),
                    size,
                    size
                );

                prop.boolValue = EditorGUI.Toggle(rect, prop.boolValue);
            }
        }
    }
}


[System.Serializable]
public struct Bool3x3
{
    public bool r0c0, r0c1, r0c2;
    public bool r1c0, r1c1, r1c2;
    public bool r2c0, r2c1, r2c2;
}