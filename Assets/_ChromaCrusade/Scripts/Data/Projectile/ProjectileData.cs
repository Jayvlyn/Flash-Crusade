using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectile", menuName = "Data/Projectile")]
public class ProjectileData : ScriptableObject
{
    public Sprite sprite;
    public bool homing;

    public int width; // diameter when circle
    public int height;

    public ProjectileShape shape;
    public enum ProjectileShape
    {
        Rectangle,
        Circle
    }
}
