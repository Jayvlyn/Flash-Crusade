using System.IO;
using UnityEngine;

public static class InventoryLoader
{
    // same path you used for test generation so your filler script can write to it
    private const string LoadPath = "Assets/_ChromaCrusade/GameData/Resources/TestInventory.json";

    public static PartInventory Load()
    {
        if (!File.Exists(LoadPath))
        {
            Debug.LogWarning($"Inventory file not found at: {LoadPath}");
            return new PartInventory(); // empty fallback
        }

        string json = File.ReadAllText(LoadPath);
        return JsonUtility.FromJson<PartInventory>(json);
    }
}