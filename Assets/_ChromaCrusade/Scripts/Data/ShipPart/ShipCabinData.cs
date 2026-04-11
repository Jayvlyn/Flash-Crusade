using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Cabin")]
public class ShipCabinData : ShipPartData
{
    public int handling => Mathf.RoundToInt(50 + mass * 0.8f);

    public override PartType PartType => PartType.Cabin;
}
