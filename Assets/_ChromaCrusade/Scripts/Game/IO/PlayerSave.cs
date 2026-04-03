using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerSave
{
    public string saveName;
    public int credits;

    public List<string> shipBuilds;
    public PartInventory partInventory;
    //public MaterialInventory materialInventory;

    public string[] pilots; //public List<PilotSave> hiredPilots;
    public string possessedPilot;

    // Faction Relationships 1 = Perfect, 0 = Enemy, 0.5 = Neutral
    public float orangeRelation;
    public float yellowRelation;
    public float greenRelation;
    public float blueRelation;
    public float indigoRelation;
    public float violetRelation;

    public void Init(string saveName)
    {
        this.saveName = saveName;

        credits = 100;
        shipBuilds = new();

        string invJson = File.ReadAllText(Paths.StartInventoryPath);
        partInventory = JsonUtility.FromJson<PartInventory>(invJson);

        pilots = new string[1];
        pilots[0] = "Sam";
        possessedPilot = pilots[0];

        orangeRelation = 0.75f;
        yellowRelation = 0.5f;
        greenRelation = 0.2f;
        blueRelation = 0.2f;
        indigoRelation = 0.5f;
        violetRelation = 0.75f;
    }
}
