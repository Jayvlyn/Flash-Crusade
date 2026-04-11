using UnityEngine;
using static SpaceObjectCreator;

public class TestFlyLoader : MonoBehaviour
{
    [SerializeField] PlayerPossessor playerPossessor;

    private void Start()
    {
        LoadTestShip();
    }

    private void LoadTestShip()
    {
        GameObject shipPrefab = Assets.Instance.gameShipPrefab;
        Ship ship = CreateSpaceObject(shipPrefab).GetComponent<Ship>();

        GameObject pilotPrefab = Assets.Instance.pilotPrefab;
        Pilot startPilot = Instantiate(pilotPrefab, ship.transform).GetComponent<Pilot>();
        startPilot.transform.localPosition = Vector3.zero;

        playerPossessor.transform.SetParent(startPilot.transform);
        playerPossessor.transform.localPosition = Vector3.zero;
        playerPossessor.pilot = startPilot;
        playerPossessor.pilot.controlledShip = ship;
        playerPossessor.pilot.possessor = playerPossessor;

        ShipGameSave save = ShipSaveLoader.GetTestGameSave();

        Sprite shipSprite = ShipSaveLoader.GetTestBuildSprite();

        ship.sprite.sprite = shipSprite;
        ship.Mass = save.mass;
        ship.handling = save.handling;
        ship.mobility = save.mobility;
        ship.Init();
    }
}
