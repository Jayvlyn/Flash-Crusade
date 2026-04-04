using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] SpaceObject playerShip;
    [SerializeField] LayeredParallax background;
    [SerializeField] InterpolatedFollowTarget followTarget;

    private void FixedUpdate()
    {
        Debug.Log("setting");
        background.referenceVelocity = playerShip.Velocity;
    }

    private void Start()
    {
        followTarget.SetTarget(playerShip.transform);
    }
}
