using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ShipBuildSave
{
    public string shipName;
    public List<PartStruct> partList;
}
