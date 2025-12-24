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
        inventory.Add("Cabin4", PartType.Cabin, 99);
        inventory.Add("Cabin5", PartType.Cabin, 99);
        inventory.Add("Cabin6", PartType.Cabin, 99);
        inventory.Add("Cabin7", PartType.Cabin, 99);
        inventory.Add("Cabin8", PartType.Cabin, 99);
        inventory.Add("Cabin9", PartType.Cabin, 99);
        inventory.Add("Cabin10", PartType.Cabin, 99);
        inventory.Add("Cabin11", PartType.Cabin, 99);
        inventory.Add("Cabin12", PartType.Cabin, 99);
        inventory.Add("Cabin13", PartType.Cabin, 99);
        inventory.Add("Cabin14", PartType.Cabin, 99);
        inventory.Add("Core1", PartType.Core, 99);
        inventory.Add("Core2", PartType.Core, 99);
        inventory.Add("Core3", PartType.Core, 99);
        inventory.Add("Core4", PartType.Core, 99);
        inventory.Add("Core5", PartType.Core, 99);
        inventory.Add("Core6", PartType.Core, 99);
        inventory.Add("Core7", PartType.Core, 99);
        inventory.Add("Core8", PartType.Core, 99);
        inventory.Add("Core9", PartType.Core, 99);
        inventory.Add("Core10", PartType.Core, 99);
        inventory.Add("Core11", PartType.Core, 99);
        inventory.Add("Core12", PartType.Core, 99);
        inventory.Add("Core13", PartType.Core, 99);
        inventory.Add("Core14", PartType.Core, 99);
        inventory.Add("Weapon1", PartType.Weapon, 99);
        inventory.Add("Weapon2", PartType.Weapon, 99);
        inventory.Add("Weapon3", PartType.Weapon, 99);
        inventory.Add("Weapon4", PartType.Weapon, 99);
        inventory.Add("Weapon5", PartType.Weapon, 99);
        inventory.Add("Weapon6", PartType.Weapon, 99);
        inventory.Add("Weapon7", PartType.Weapon, 99);
        inventory.Add("Weapon8", PartType.Weapon, 99);
        inventory.Add("Weapon9", PartType.Weapon, 99);
        inventory.Add("Weapon10", PartType.Weapon, 99);
        inventory.Add("Weapon11", PartType.Weapon, 99);
        inventory.Add("Weapon12", PartType.Weapon, 99);
        inventory.Add("Weapon13", PartType.Weapon, 99);
        inventory.Add("Weapon14", PartType.Weapon, 99);
        inventory.Add("Weapon15", PartType.Weapon, 99);
        inventory.Add("Weapon16", PartType.Weapon, 99);
        inventory.Add("Weapon17", PartType.Weapon, 99);
        inventory.Add("Weapon18", PartType.Weapon, 99);
        inventory.Add("Weapon19", PartType.Weapon, 99);
        inventory.Add("Weapon20", PartType.Weapon, 99);
        inventory.Add("Weapon21", PartType.Weapon, 99);
        inventory.Add("Wing1", PartType.Wing, 99);
        inventory.Add("Wing2", PartType.Wing, 99);
        inventory.Add("Wing3", PartType.Wing, 99);
        inventory.Add("Wing4", PartType.Wing, 99);
        inventory.Add("Wing5", PartType.Wing, 99);
        inventory.Add("Wing6", PartType.Wing, 99);
        inventory.Add("Wing7", PartType.Wing, 99);
        inventory.Add("Wing8", PartType.Wing, 99);
        inventory.Add("Wing9", PartType.Wing, 99);
        inventory.Add("Wing10", PartType.Wing, 99);
        inventory.Add("Wing11", PartType.Wing, 99);
        inventory.Add("Wing12", PartType.Wing, 99);
        inventory.Add("Wing13", PartType.Wing, 99);
        inventory.Add("Wing14", PartType.Wing, 99);
        inventory.Add("Wing15", PartType.Wing, 99);
        inventory.Add("Wing16", PartType.Wing, 99);
        inventory.Add("Wing17", PartType.Wing, 99);
        inventory.Add("Wing18", PartType.Wing, 99);
        inventory.Add("Wing19", PartType.Wing, 99);
        inventory.Add("Wing20", PartType.Wing, 99);
        inventory.Add("Wing21", PartType.Wing, 99);
        inventory.Add("Wing22", PartType.Wing, 99);
        inventory.Add("Wing23", PartType.Wing, 99);
        inventory.Add("Wing24", PartType.Wing, 99);
        inventory.Add("Wing25", PartType.Wing, 99);
        inventory.Add("Wing26", PartType.Wing, 99);
        inventory.Add("Wing27", PartType.Wing, 99);
        inventory.Add("Wing28", PartType.Wing, 99);
        inventory.Add("Wing29", PartType.Wing, 99);
        inventory.Add("Wing30", PartType.Wing, 99);
        inventory.Add("Wing31", PartType.Wing, 99);
        inventory.Add("Wing32", PartType.Wing, 99);
        inventory.Add("Wing33", PartType.Wing, 99);
        inventory.Add("Wing34", PartType.Wing, 99);
        inventory.Add("Wing35", PartType.Wing, 99);
        inventory.Add("Wing36", PartType.Wing, 99);
        inventory.Add("Wing37", PartType.Wing, 99);
        inventory.Add("Wing38", PartType.Wing, 99);
        inventory.Add("Wing39", PartType.Wing, 99);
        inventory.Add("Wing40", PartType.Wing, 99);
        inventory.Add("Wing41", PartType.Wing, 99);
        inventory.Add("Wing42", PartType.Wing, 99);
        inventory.Add("Wing43", PartType.Wing, 99);
        inventory.Add("Wing44", PartType.Wing, 99);
        inventory.Add("Wing45", PartType.Wing, 99);
        inventory.Add("Wing46", PartType.Wing, 99);
        inventory.Add("Wing47", PartType.Wing, 99);
        inventory.Add("Wing48", PartType.Wing, 99);
        inventory.Add("Wing49", PartType.Wing, 99);
        inventory.Add("Wing50", PartType.Wing, 99);
        inventory.Add("Wing51", PartType.Wing, 99);
        inventory.Add("Wing52", PartType.Wing, 99);

        string json = JsonUtility.ToJson(inventory, true);

        File.WriteAllText(OutputPath, json);
        AssetDatabase.Refresh();

        Debug.Log($"Test inventory saved to {OutputPath}");
    }
}
