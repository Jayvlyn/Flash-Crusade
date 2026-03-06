#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class InventoryGenerator
{
    private const string OutputPath = "Assets/_ChromaCrusade/GameData/Resources/TestInventory.json";

    [MenuItem("Tools/Generate Test Inventory")]
    public static void Generate()
    {
        var inventory = new PartInventory();

        // Fake test data
        inventory.Add("Cabin1", PartType.Cabin, 99);
        inventory.Add("Cabin2", PartType.Cabin, 99);
        inventory.Add("Cabin3", PartType.Cabin, 99);
        inventory.Add("Core1", PartType.Core, 99);
        inventory.Add("Core2", PartType.Core, 99);
        inventory.Add("Core3", PartType.Core, 99);
        inventory.Add("Core4", PartType.Core, 99);
        inventory.Add("Core5", PartType.Core, 99);
        inventory.Add("Core6", PartType.Core, 99);
        inventory.Add("Weapon1", PartType.Weapon, 99);
        inventory.Add("Weapon2", PartType.Weapon, 99);
        inventory.Add("Weapon3", PartType.Weapon, 99);
        inventory.Add("Wing1", PartType.Wing, 99);
        inventory.Add("Wing2", PartType.Wing, 99);
        inventory.Add("Wing3", PartType.Wing, 99);
        inventory.Add("Wing4", PartType.Wing, 99);
        inventory.Add("Wing5", PartType.Wing, 99);
        inventory.Add("Capacitor1", PartType.Utility, 99);
        inventory.Add("Capacitor2", PartType.Utility, 99);
        inventory.Add("Capacitor3", PartType.Utility, 99);
        inventory.Add("Dock1", PartType.Utility, 99);
        inventory.Add("Dock2", PartType.Utility, 99);
        inventory.Add("Enhancer1", PartType.Utility, 99);
        inventory.Add("Enhancer2", PartType.Utility, 99);

        string json = JsonUtility.ToJson(inventory, true);

        File.WriteAllText(OutputPath, json);
        AssetDatabase.Refresh();

        Debug.Log($"Test inventory saved to {OutputPath}");
    }
}
#endif