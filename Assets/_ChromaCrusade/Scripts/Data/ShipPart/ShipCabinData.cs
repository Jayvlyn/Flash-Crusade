using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Cabin")]
public class ShipCabinData : ShipPartData
{
    public int handling => Mathf.RoundToInt(67 + mass * 0.5f);

    public override PartType PartType => PartType.Cabin;
}
