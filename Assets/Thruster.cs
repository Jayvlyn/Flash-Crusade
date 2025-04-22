using UnityEngine;

public class Thruster : MonoBehaviour
{
    private Vector2 activePosition;
    private Vector2 inactivePosition;

    private void Awake()
    {
        activePosition = transform.localPosition;
        inactivePosition = activePosition;
        inactivePosition.y += 4;
    }
}
