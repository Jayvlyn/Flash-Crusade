using System.IO;
using UnityEngine;

public static class Paths
{
    public static string MainPath => Application.persistentDataPath;

    public static string ShipPresetsPath => Path.Combine(MainPath, "ShipPresets");
    public static string ShipPresetSpritesPath => Path.Combine(ShipPresetsPath, "ShipSprites");
    public static string ShipPresetDataPath => Path.Combine(ShipPresetsPath, "ShipData");

    public static string PlayerSavePath(string saveName) => Path.Combine(MainPath, saveName);

    public static string ShipBuildsPath(string saveName) => Path.Combine(PlayerSavePath(saveName), "ShipBuilds");

    public static string ShipBuildSpritesPath(string saveName) => Path.Combine(ShipBuildsPath(saveName), "Sprites");
    public static string ShipBuildDataPath(string saveName) =>    Path.Combine(ShipBuildsPath(saveName), "Data");
}
