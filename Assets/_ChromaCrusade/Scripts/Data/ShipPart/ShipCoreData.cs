using UnityEngine;

[CreateAssetMenu(fileName = "NewShipPart", menuName = "Data/Ship Core")]
public class ShipCoreData : ShipPartData
{
    public int energy;

    public override PartType PartType => PartType.Core;

    //public override void Apply(ImporterPart importer)
    //{
    //    base.Apply(importer);
    //    energy = importer.energy;
    //}
}
