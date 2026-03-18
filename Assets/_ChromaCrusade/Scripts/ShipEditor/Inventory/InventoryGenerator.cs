#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class InventoryGenerator
{
    [MenuItem("Tools/Generate Start Inventory")]
    public static void Generate()
    {
        Directory.CreateDirectory(Paths.ShipPartsPath);
        var inventory = new PartInventory();

        inventory.Add("Cabin39", PartType.Cabin, 1);
        inventory.Add("Cabin46", PartType.Cabin, 1);
        inventory.Add("Cabin57", PartType.Cabin, 1);
        inventory.Add("Cabin59", PartType.Cabin, 1);
        inventory.Add("Core27", PartType.Core, 2);
        inventory.Add("Core33", PartType.Core, 2);
        inventory.Add("Core32", PartType.Core, 2);
        inventory.Add("Core30", PartType.Core, 2);
        inventory.Add("Weapon0", PartType.Weapon, 2);
        inventory.Add("Weapon1", PartType.Weapon, 2);
        inventory.Add("Weapon11", PartType.Weapon, 2);
        inventory.Add("Wing0", PartType.Wing, 2);
        inventory.Add("Wing33", PartType.Wing, 2);
        inventory.Add("Wing38", PartType.Wing, 2);
        inventory.Add("Wing49", PartType.Wing, 2);
        inventory.Add("Wing54", PartType.Wing, 2);
        inventory.Add("Wing58", PartType.Wing, 2);
        inventory.Add("Wing64", PartType.Wing, 2);
        inventory.Add("Wing73", PartType.Wing, 2);
        inventory.Add("Wing76", PartType.Wing, 2);
        inventory.Add("Wing94", PartType.Wing, 2);
        inventory.Add("Capacitor13", PartType.Utility, 2);
        inventory.Add("Capacitor14", PartType.Utility, 2);

        string json = JsonUtility.ToJson(inventory, true);

        File.WriteAllText(Paths.StartInventoryPath, json);
        AssetDatabase.Refresh();
    }
}
#endif