using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Cabin")]
public class ShipCabinData : ShipPartData
{
    public float handling;

    public override PartType PartType => PartType.Cabin;
}
