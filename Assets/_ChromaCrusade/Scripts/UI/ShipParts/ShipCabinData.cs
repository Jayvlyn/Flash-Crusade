using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Cabin")]
public class ShipCabinData : ShipPartData
{
    public float handling;

    public override PartType PartType => PartType.Cabin;

    public override void Apply(ImporterPart importer)
    {
        base.Apply(importer);
        handling = importer.handling;
    }
}
