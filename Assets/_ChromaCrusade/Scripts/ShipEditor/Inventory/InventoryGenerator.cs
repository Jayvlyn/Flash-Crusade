#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class InventoryGenerator
{
    [MenuItem("Tools/Generate Test Inventory")]
    public static void Generate()
    {
        Directory.CreateDirectory(Paths.ShipPartsPath);
        var inventory = new PartInventory();

        inventory.Add("Cabin1", PartType.Cabin, 10);
        inventory.Add("Cabin2", PartType.Cabin, 10);
        inventory.Add("Cabin3", PartType.Cabin, 10);
        inventory.Add("Core1", PartType.Core, 10);
        inventory.Add("Core2", PartType.Core, 10);
        inventory.Add("Core3", PartType.Core, 10);
        inventory.Add("Core4", PartType.Core, 10);
        inventory.Add("Core5", PartType.Core, 10);
        inventory.Add("Core6", PartType.Core, 10);
        inventory.Add("Weapon1", PartType.Weapon, 10);
        inventory.Add("Weapon2", PartType.Weapon, 10);
        inventory.Add("Weapon3", PartType.Weapon, 10);
        inventory.Add("Wing1", PartType.Wing, 10);
        inventory.Add("Wing2", PartType.Wing, 10);
        inventory.Add("Wing3", PartType.Wing, 10);
        inventory.Add("Wing4", PartType.Wing, 10);
        inventory.Add("Wing5", PartType.Wing, 10);
        inventory.Add("Capacitor1", PartType.Utility, 10);
        inventory.Add("Capacitor2", PartType.Utility, 10);
        inventory.Add("Capacitor3", PartType.Utility, 10);
        inventory.Add("Dock1", PartType.Utility, 10);
        inventory.Add("Dock2", PartType.Utility, 10);
        inventory.Add("Enhancer1", PartType.Utility, 10);
        inventory.Add("Enhancer2", PartType.Utility, 10);

        string json = JsonUtility.ToJson(inventory, true);

        File.WriteAllText(Paths.TestInventoryPath, json);
        AssetDatabase.Refresh();
    }
}
#endif