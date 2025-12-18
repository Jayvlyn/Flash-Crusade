using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Utility")]
public class ShipUtilityData : ShipPartData
{
    public override PartType PartType => PartType.Utility;

    public enum UtilityType
    { // 0 is "select" in importer
        Dock = 1,       // other ships can dock here
        Reflector = 2,  // bullets get reflected back
        Charger = 3,    // Recharges energy passively
        Converter = 4,  // Converts ice to energy
        Repulsor = 5,   // Launches away nearby enemies
        TractorBeam = 6 // Pulls in objects/enemies
    }
    public UtilityType utilityType;

    //public override void Apply(ImporterPart importer)
    //{
    //    base.Apply(importer);
    //    utilityType = (UtilityType)(int)importer.utilityType;
    //}
}