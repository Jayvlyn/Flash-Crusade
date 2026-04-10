using UnityEngine;
using static SpaceObjectCreator;

public class GameShipSaveLoader : MonoBehaviour
{
    [SerializeField] PlayerPossessor playerPossessor;

    private void Start()
    {
        LoadNewGame();
    }

    private void LoadNewGame()
    {
        PlayerSave activeSave = PlayerSaveManager.ActiveSave;
        if (activeSave == null) return;

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

        string shipName = activeSave.shipBuilds[0];

        ShipGameSave save = ShipSaveLoader.GetShipGameSave(shipName);

        Sprite shipSprite = ShipSaveLoader.GetShipBuildSprite(shipName);

        ship.sprite.sprite = shipSprite;
        ship.Mass = save.mass;
        ship.handling = save.handling;
        ship.mobility = save.mobility;
        ship.Init();

    }
}
