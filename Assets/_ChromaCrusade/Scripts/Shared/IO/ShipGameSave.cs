using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ShipGameSave
{
    public string shipName;
    public int maxEnergy;
    public int mobility;
    public int handling;
    public int mass;
    public List<PositionName> weapons;
}
