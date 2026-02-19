using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Utility")]
public class ShipUtilityData : ShipPartData
{
    public override PartType PartType => PartType.Utility;

    public enum UtilityType
    { // 0 is "select" in importer
        Dock = 1,       // Other ships can dock here, more options when piloting ship. "space station"
        Enhancer = 2,   // Enhances connected weapons
        Capacitor = 3,    // Recharges energy passively

        //StationCPU = 4, // Classifies ship build as a space station
        //Converter = 4,  // Converts ice to energy
        //Repulsor = 5,   // Launches away nearby enemies
        //TractorBeam = 6, // Pulls in objects/enemies
    }
    public UtilityType utilityType;
}