using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] LayeredParallax background;
    [SerializeField] PlayerPossessor player;

    private void FixedUpdate()
    {
        background.referenceVelocity = player.pilot.controlledShip.Velocity;
    }

}
