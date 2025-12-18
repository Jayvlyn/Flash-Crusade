using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Wing")]
public class ShipWingData : ShipPartData
{
    public int mobility;

    public override PartType PartType => PartType.Wing;
}
