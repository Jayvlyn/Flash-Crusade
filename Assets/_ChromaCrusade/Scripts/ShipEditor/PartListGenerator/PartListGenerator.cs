
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class PartListGenerator
{
    public const string PartsRootFolder = "Assets/_ChromaCrusade/GameData/Resources/Parts";

    [MenuItem("Tools/Generate Ship Part List")]
    public static void Generate()
    {
        Directory.CreateDirectory(Paths.ShipPartsPath);

        var result = new ShipPartList();

        string[] guids = AssetDatabase.FindAssets("t:ShipPartData", new[] { PartsRootFolder });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ShipPartData>(path);

            if (asset == null)
            {
                Debug.LogWarning($"Failed to load asset at path: {path}");
                continue;
            }

            switch (asset)
            {
                case ShipCabinData:
                    result.cabins.Add(asset.name);
                    break;
                case ShipCoreData:
                    result.cores.Add(asset.name);
                    break;
                case ShipUtilityData:
                    result.utilities.Add(asset.name);
                    break;
                case ShipWingData:
                    result.wings.Add(asset.name);
                    break;
                case ShipWeaponData:
                    result.weapons.Add(asset.name);
                    break;

                default:
                    Debug.LogWarning($"Unhandled type: {asset.GetType().Name}");
                    break;
            }
        }

        string json = JsonUtility.ToJson(result, true);

        File.WriteAllText(Paths.PartListPath, json);
        AssetDatabase.Refresh();
    }
}
#endif
