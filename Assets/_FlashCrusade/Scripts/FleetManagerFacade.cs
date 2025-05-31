using System.Linq;
using UnityEngine;

public class FleetManagerFacade
{
    public Leader leader;
    public Ship leaderShip;
    public Fleet fleet;

    public float fleetPosTick = 0.5f;
    private float fleetPosTickTimer;

    public FleetManagerFacade(Leader leader, float shipSpacing)
    {
        this.leader = leader;
        this.leaderShip = leader.ship;

        fleet = new Fleet(leaderShip, shipSpacing);

        Ally[] allyComps = Object.FindObjectsByType<Ally>(FindObjectsSortMode.None);
        fleet.ships = allyComps.Cast<AIAgent>().ToList();
    }

    public void Update()
    {
        if (fleetPosTickTimer > 0)
        {
            fleetPosTickTimer -= Time.deltaTime;
        }
        else
        {
            fleetPosTickTimer = fleetPosTick;
        }
        fleet.UpdateLocalFleetPositions(); // move back into tick later
    }

    public void UpdateFleetMoveTargets()
    {
        for (int i = 0; i < fleet.ships.Count; i++)
        {
            Vector2 worldPos = (Vector2)leader.transform.position + fleet.localFleetPositions[i];

            fleet.ships[i].MoveTarget = worldPos;
        }
    }
}
