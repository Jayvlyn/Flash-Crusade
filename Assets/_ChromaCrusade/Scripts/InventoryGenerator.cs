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
        inventory.Add("CoreTest1", PartType.Core, 2);
        inventory.Add("CoreTest2", PartType.Core, 2);
        inventory.Add("CoreTest3", PartType.Core, 2);
        inventory.Add("CoreTest4", PartType.Core, 2);
        inventory.Add("CoreTest5", PartType.Core, 2);
        inventory.Add("CoreTest6", PartType.Core, 2);
        inventory.Add("CoreTest7", PartType.Core, 2);
        inventory.Add("CoreTest8", PartType.Core, 2);
        inventory.Add("CoreTest9", PartType.Core, 2);
        inventory.Add("CoreTest10", PartType.Core, 2);
        inventory.Add("CoreTest11", PartType.Core, 2);
        inventory.Add("CoreTest12", PartType.Core, 2);
        inventory.Add("CoreTest13", PartType.Core, 2);
        inventory.Add("CoreTest14", PartType.Core, 2);
        inventory.Add("CoreTest15", PartType.Core, 2);
        inventory.Add("CoreTest16", PartType.Core, 2);
        inventory.Add("CoreTest17", PartType.Core, 2);
        inventory.Add("CoreTest18", PartType.Core, 2);
        inventory.Add("CoreTest19", PartType.Core, 2);
        inventory.Add("CoreTest20", PartType.Core, 2);

        string json = JsonUtility.ToJson(inventory, true);

        File.WriteAllText(OutputPath, json);
        AssetDatabase.Refresh();

        Debug.Log($"Test inventory saved to {OutputPath}");
    }
}
