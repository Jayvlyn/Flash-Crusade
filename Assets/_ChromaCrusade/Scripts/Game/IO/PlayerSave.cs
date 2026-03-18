using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSave
{
    public string firstCharacterName;
    public int credits;

    public List<ShipSave> shipBuilds;
    public PartInventory partInventory;
    //public MaterialInventory materialInventory;

    public string[] hiredPilots; //public List<PilotSave> hiredPilots;
    public string possessedPilot;

    // Faction Relationships 1 = Perfect, 0 = Enemy, 0.5 = Neutral
    public float orangeRelation = 0.75f;
    public float yellowRelation = 0.5f;
    public float greenRelation = 0.2f;
    public float blueRelation = 0.2f;
    public float indigoRelation = 0.5f;
    public float violetRelation = 0.75f;
}
