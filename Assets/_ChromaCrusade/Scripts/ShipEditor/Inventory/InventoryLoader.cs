using System.IO;
using UnityEngine;

public static class InventoryLoader
{
    public static PartInventory Load()
    {
        if (!File.Exists(Paths.StartInventoryPath))
            return new PartInventory(); // empty fallback
        
        string json = File.ReadAllText(Paths.StartInventoryPath);
        return JsonUtility.FromJson<PartInventory>(json);
    }

    public static ShipPartList LoadFullList()
    {
        if (!File.Exists(Paths.PartListPath))
            return new ShipPartList(); // empty fallback
        
        string json = File.ReadAllText(Paths.PartListPath);
        return JsonUtility.FromJson<ShipPartList>(json);
    }
}