using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] SpaceObject playerShip;

    [SerializeField] LayeredParallax background;
    [SerializeField] TransformFollower backgroundTF;

    [SerializeField] InterpolatedFollowTarget followTarget;

    [SerializeField] PhysicsManager physicsManager;

    private void FixedUpdate()
    {
        background.referenceVelocity = playerShip.Velocity;
    }

    private void Awake()
    {
        followTarget.SetTarget(playerShip.transform);

        backgroundTF.target = playerShip.transform;
        physicsManager.player = playerShip.transform;
    }
}
