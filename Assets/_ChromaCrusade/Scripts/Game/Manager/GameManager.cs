using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] LayeredParallax background;
    [SerializeField] PlayerPossessor player;

    private void FixedUpdate()
    {
        background.referenceVelocity = player.pilot.controlledShip.Velocity;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<CancelInputEvent>(OnCancelInput);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CancelInputEvent>(OnCancelInput);
    }

    private void OnCancelInput(CancelInputEvent e)
    {
        SceneManager.LoadScene("Scene_Builder");
    }

}
