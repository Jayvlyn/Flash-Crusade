using System.IO;
using UnityEngine;

public static class Paths
{
    public static string Persistent => Application.persistentDataPath;
    public static string Streaming => Application.streamingAssetsPath;

    public static string ShipPartsPath => Path.Combine(Streaming, "ShipParts");
    public static string PartListPath => Path.Combine(ShipPartsPath, "PartList.json");
    public static string StartInventoryPath => Path.Combine(ShipPartsPath, "StartInventory.json");

    public static string PlayerPresetsPath => Path.Combine(Persistent, "PlayerPresets");
    public static string PlayerPresetSpritesPath => Path.Combine(PlayerPresetsPath, "PlayerPresetSprites");
    public static string PlayerPresetDataPath => Path.Combine(PlayerPresetsPath, "PlayerPresetData");

    public static string DevPresetsPath => Path.Combine(Streaming, "DevPresets");
    public static string DevPresetSpritesPath => Path.Combine(DevPresetsPath, "DevPresetSprites");
    public static string DevPresetDataPath => Path.Combine(DevPresetsPath, "DevPresetData");

    public static string TestBuildPath => Path.Combine(Persistent, "TestBuild");

    public static string PlayerSavesPath => Path.Combine(Persistent, "PlayerSaves");
    public static string PlayerSaveFolder(string saveName) => Path.Combine(PlayerSavesPath, saveName);
    public static string PlayerSavePath(string saveName) => Path.Combine(PlayerSaveFolder(saveName), $"{saveName}.json");

    public static string ShipPath(string saveName) => Path.Combine(PlayerSaveFolder(saveName), "Ships");

    public static string ShipSpritesPath(string saveName) => Path.Combine(ShipPath(saveName), "Sprites");
    public static string ShipBuildDataPath(string saveName) =>    Path.Combine(ShipPath(saveName), "BuildData");
    public static string ShipGameDataPath(string saveName) =>     Path.Combine(ShipPath(saveName), "GameData");
}
