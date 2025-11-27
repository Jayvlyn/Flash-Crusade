using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Part")]
public class ShipPartData : ScriptableObject
{
    [Header("Visual")]
    public Sprite sprite;

    [Tooltip("Which of the 9 slots in the 3x3 does the sprite take up")]
    public Bool3x3 slots;

    [Header("Stats")]
    public float mass;
}