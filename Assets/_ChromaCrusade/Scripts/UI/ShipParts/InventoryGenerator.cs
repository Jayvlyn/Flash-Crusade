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
        inventory.Add("CabinTest1", PartType.Cabin, 1);
        inventory.Add("CabinTest2", PartType.Cabin, 3);
        inventory.Add("CabinTest3", PartType.Cabin, 10);
        inventory.Add("WingTest1", PartType.Wing, 4);
        inventory.Add("WingTest2", PartType.Wing, 4);
        inventory.Add("WingTest3", PartType.Wing, 4);
        inventory.Add("GunTest1", PartType.Weapon, 1);
        inventory.Add("GunTest2", PartType.Weapon, 5);
        inventory.Add("GunTest3", PartType.Weapon, 7);
        inventory.Add("GunTest4", PartType.Weapon, 20);

        string json = JsonUtility.ToJson(inventory, true);

        File.WriteAllText(OutputPath, json);
        AssetDatabase.Refresh();

        Debug.Log($"Test inventory saved to {OutputPath}");
    }
}
