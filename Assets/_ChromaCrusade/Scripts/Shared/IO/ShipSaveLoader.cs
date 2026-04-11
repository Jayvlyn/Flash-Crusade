using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;
using UnityEngine.UI;

public static class ShipSaveLoader
{
    #region Public API
    public static void SaveShipBuild(UIShipData shipData, IEnumerable<EditorShipPart> parts)
    {
        PlayerSave playerSave = PlayerSaveManager.ActiveSave;

        if(!playerSave.shipBuilds.Contains(shipData.shipName))
            playerSave.shipBuilds.Add(shipData.shipName);

        SaveShipBuildData(shipData.shipName, parts);

        SaveShipGameData(shipData, parts);

        SaveShipBuildTexture(shipData);

        PlayerSaveManager.SaveToJson(playerSave); // maybe not needed here, needs to happen at some point though
    }

    private static void SaveShipBuildTexture(UIShipData shipData)
    {
        string path = Paths.ShipSpritesPath(PlayerSaveManager.ActiveSave.saveName);
        SaveShipTexture(shipData, path);
    }

    private static void SaveShipPresetTexture(UIShipData shipData, bool dev = false)
    {
        Directory.CreateDirectory(Paths.PlayerPresetSpritesPath);
        Directory.CreateDirectory(Paths.DevPresetSpritesPath);
        string path = dev ? Paths.DevPresetSpritesPath : Paths.PlayerPresetSpritesPath;
        SaveShipTexture(shipData, path);
    }

    private static void SaveShipTestTexture(UIShipData shipData)
    {
        string path = Paths.TestBuildPath;
        Directory.CreateDirectory(path);
        SaveShipTexture(shipData, path);
    }

    private static void SaveShipTexture(UIShipData shipData, string path)
    {
        byte[] pngBytes = shipData.shipSprite.texture.EncodeToPNG();

        Directory.CreateDirectory(path);

        path = Path.Combine(path, $"Test_Game.png");

        File.WriteAllBytes(path, pngBytes);

        //Object.Destroy(shipData.shipSprite.texture);
    }

    public static void SaveBuildAsTest(UIShipData shipData, IEnumerable<EditorShipPart> parts)
    {
        SaveShipTestTexture(shipData);

        ShipBuildSave buildSave = ConstructShipBuildSave(shipData.shipName, parts);
        ShipGameSave gameSave = ConstructShipGameSave(shipData.shipName, parts);

        string buildJson = JsonUtility.ToJson(buildSave, true);
        string gameJson = JsonUtility.ToJson(gameSave, true);

        File.WriteAllText(
            Path.Combine(Paths.TestBuildPath, $"Test_Build.json"),
            buildJson);

        File.WriteAllText(
            Path.Combine(Paths.TestBuildPath, $"Test_Game.json"),
            gameJson);
    }

    public static void SaveBuildAsPreset(UIShipData shipData, IEnumerable<EditorShipPart> parts, bool dev = false)
    {
        SaveShipPresetTexture(shipData, dev);

        var shipSave = ConstructShipBuildSave(shipData.shipName, parts);

        string json = JsonUtility.ToJson(shipSave, true);

        Directory.CreateDirectory(Paths.PlayerPresetDataPath);
        Directory.CreateDirectory(Paths.DevPresetDataPath);
        string path = dev ? Paths.DevPresetDataPath : Paths.PlayerPresetDataPath;

        File.WriteAllText(
            Path.Combine(path, $"{shipData.shipName}.json"), 
            json );
    }

    public static void SaveShipBuildData(string shipName, IEnumerable<EditorShipPart> parts)
    {
        var shipSave = ConstructShipBuildSave(shipName, parts);

        string json = JsonUtility.ToJson(shipSave, true);

        string path = Paths.ShipBuildDataPath(PlayerSaveManager.ActiveSave.saveName);

        Directory.CreateDirectory(path);

        File.WriteAllText(
            Path.Combine(path, $"{shipName}.json"),
            json);
    }

    public static void SaveShipGameData(UIShipData shipData, IEnumerable<EditorShipPart> parts)
    { 
        ShipGameSave gameSave = ConstructShipGameSave(shipData.shipName, parts);

        string json = JsonUtility.ToJson(gameSave, true);

        string path = Paths.ShipGameDataPath(PlayerSaveManager.ActiveSave.saveName);

        Directory.CreateDirectory(path);

        File.WriteAllText(
            Path.Combine(path, $"{shipData.shipName}.json"),
            json);
    }

    public static ShipBuildSave GetShipPreset(string presetName, bool dev = false)
    {
        Directory.CreateDirectory(Paths.DevPresetDataPath);
        Directory.CreateDirectory(Paths.PlayerPresetDataPath);
        string path = dev ? Paths.DevPresetDataPath : Paths.PlayerPresetDataPath;

        return GetShipBuildSave(
            Path.Combine(path, 
            $"{presetName}.json"));
    }

    public static ShipBuildSave GetShipBuildSave(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Ship build save not found: " + path);
            return new ShipBuildSave();
        }

        string json = File.ReadAllText(path);

        ShipBuildSave shipBuildSave = JsonUtility.FromJson<ShipBuildSave>(json);

        if (shipBuildSave.partList == null)
            return new ShipBuildSave();

        return shipBuildSave;
    }

    public static ShipGameSave GetShipGameSave(string shipName)
    {
        string path = Path.Combine(Paths.ShipGameDataPath(PlayerSaveManager.ActiveSave.saveName), $"{shipName}.json");

        if(!File.Exists(path))
        {
            Debug.LogError("Ship game save not found: " + path);
            return new ShipGameSave();
        }

        string json = File.ReadAllText(path);

        ShipGameSave shipGameSave = JsonUtility.FromJson<ShipGameSave>(json);

        return shipGameSave;
    }

    public static ShipGameSave GetTestGameSave()
    {
        string path = Path.Combine(Paths.TestBuildPath, $"Test_Game.json");

        if (!File.Exists(path))
        {
            Debug.LogError("Ship game save not found: " + path);
            return new ShipGameSave();
        }

        string json = File.ReadAllText(path);

        ShipGameSave shipGameSave = JsonUtility.FromJson<ShipGameSave>(json);

        return shipGameSave;
    }

    #endregion

    static ShipBuildSave ConstructShipBuildSave(string shipName, IEnumerable<EditorShipPart> parts)
    {
        ShipBuildSave shipBuildSave = new ShipBuildSave
        {
            shipName = shipName,
            partList = new List<PartStruct>()
        };

        foreach (var part in parts)
        {
            shipBuildSave.partList.Add(new PartStruct
            {
                partName = part.partData.name,
                xPos = part.position.x,
                yPos = part.position.y,
                xFlipped = part.xFlipped,
                yFlipped = part.yFlipped,
                rotation = Mathf.RoundToInt(part.Rotation) % 360
            });
        }

        return shipBuildSave;
    }

    static ShipGameSave ConstructShipGameSave(string shipName, IEnumerable<EditorShipPart> parts)
    {
        // this needs some kind of thing to track where the center of the ship was so we know where to place
        // the weapons on the ship. we save positions of the ship used in builder, but the centerpoint from builder
        // is lost in this data, no way to restore ship parts without conserving the center point 

        ShipGameSave shipGameSave = new ShipGameSave {shipName = shipName};

        foreach(var part in parts)
        {
            shipGameSave.mass += part.partData.mass;

            if (part.partData is ShipCoreData core)
                shipGameSave.maxEnergy += core.energy;
            else if (part.partData is ShipCabinData cabin)
                shipGameSave.handling += cabin.handling;
            else if (part.partData is ShipWingData wing)
                shipGameSave.mobility += wing.mobility;
            else if (part.partData is ShipWeaponData weapon)
            {
                if (shipGameSave.weapons == null) 
                    shipGameSave.weapons = new();

                shipGameSave.weapons.Add(new PositionName { 
                    position = part.position, 
                    name = part.partData.name
                });
            }
        }

        return shipGameSave;
    }

    public static Sprite GetShipBuildSprite(string shipName)
    {
        string path = Path.Combine(Paths.ShipSpritesPath(PlayerSaveManager.ActiveSave.saveName), $"{shipName}.png");

        return GetShipSprite(path);
    }

    public static Sprite GetTestBuildSprite()
    {
        string path = Path.Combine(Paths.TestBuildPath, $"Test_Game.png");

        return GetShipSprite(path);
    }

    static Sprite GetShipSprite(string path)
    {
        byte[] spriteBytes = File.ReadAllBytes(path);

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(spriteBytes); // auto-resizes so 2,2 doesnt matter
        texture.filterMode = FilterMode.Point;

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            9
        );

        return sprite;
    }
}
