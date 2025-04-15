using UnityEngine;

[CreateAssetMenu(fileName = "Bullet", menuName = "Game Data/Bullet")]
public class Bullet : ScriptableObject
{
    public Sprite sprite;

    [Tooltip("Damage dealt to a ship hit by this bullet.")]
    public float damage = 10;

    [Tooltip("How many ships this bullet can pass through before being destroyed."),Range(0,20)]
    public int piercing = 0;

    [Tooltip("How long the bullet will stay airborne before being destroyed.")]
    public float lifetime = 20;

    [Tooltip("How fast bullets turn and move towards targets (0 = No homing)"), Range(0, 30)]
    public float homing = 0;
}
