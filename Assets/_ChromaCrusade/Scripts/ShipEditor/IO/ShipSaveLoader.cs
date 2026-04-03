using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;

public class ShipSaveLoader
{
    #region Public API

    public ShipSaveLoader()
    {
        Directory.CreateDirectory(Paths.PlayerPresetSpritesPath);
        Directory.CreateDirectory(Paths.PlayerPresetDataPath);

        Directory.CreateDirectory(Paths.DevPresetSpritesPath);
        Directory.CreateDirectory(Paths.DevPresetDataPath);
    }

    public void SaveShipBuild(UIShipData shipData, IEnumerable<EditorShipPart> parts)
    {
        PlayerSave playerSave = PlayerSaveManager.ActiveSave;

        if(!playerSave.shipBuilds.Contains(shipData.shipName))
            playerSave.shipBuilds.Add(shipData.shipName);

        SaveShipBuildData(shipData.shipName, parts);

        SaveShipGameData(shipData, parts);

        SaveShipBuildTexture(shipData);

        PlayerSaveManager.SaveToJson(playerSave); // maybe not needed here, needs to happen at some point though
    }

    private void SaveShipBuildTexture(UIShipData shipData)
    {
        string path = Paths.ShipBuildSpritesPath(PlayerSaveManager.ActiveSave.saveName);
        SaveShipTexture(shipData, path);
    }

    private void SaveShipPresetTexture(UIShipData shipData, bool dev = false)
    {
        string path = dev ? Paths.DevPresetSpritesPath : Paths.PlayerPresetSpritesPath;
        SaveShipTexture(shipData, path);
    }

    private void SaveShipTexture(UIShipData shipData, string path)
    {
        byte[] pngBytes = shipData.shipSprite.texture.EncodeToPNG();

        Directory.CreateDirectory(path);

        path = Path.Combine(path, $"{shipData.shipName}.png");

        File.WriteAllBytes(path, pngBytes);

        //Object.Destroy(shipData.shipSprite.texture);
    }

    public void SaveBuildAsPreset(UIShipData shipData, IEnumerable<EditorShipPart> parts, bool dev = false)
    {
        SaveShipPresetTexture(shipData, dev);

        // Save Build Data
        var shipSave = ConstructShipBuildSave(shipData.shipName, parts);

        string json = JsonUtility.ToJson(shipSave, true);

        string path = dev ? Paths.DevPresetDataPath : Paths.PlayerPresetDataPath;

        File.WriteAllText(
            Path.Combine(path, $"{shipData.shipName}.json"), 
            json );
    }

    public void SaveShipBuildData(string shipName, IEnumerable<EditorShipPart> parts)
    {
        var shipSave = ConstructShipBuildSave(shipName, parts);

        string json = JsonUtility.ToJson(shipSave, true);

        string path = Paths.ShipBuildDataPath(PlayerSaveManager.ActiveSave.saveName);

        Directory.CreateDirectory(path);

        File.WriteAllText(
            Path.Combine(path, $"{shipName}.json"),
            json);
    }

    public void SaveShipGameData(UIShipData shipData, IEnumerable<EditorShipPart> parts)
    { 
        ShipGameSave gameSave = ConstructShipGameSave(shipData.shipName, parts);

        string json = JsonUtility.ToJson(gameSave, true);

        string path = Paths.ShipGameDataPath(PlayerSaveManager.ActiveSave.saveName);

        Directory.CreateDirectory(path);

        File.WriteAllText(
            Path.Combine(path, $"{shipData.shipName}.json"),
            json);
    }

    public ShipBuildSave GetShipPreset(string presetName, bool dev = false)
    {
        string path = dev ? Paths.DevPresetDataPath : Paths.PlayerPresetDataPath;

        return GetShipSave(
            Path.Combine(path, 
            $"{presetName}.json"));
    }


    //public ShipSave GetShipBuild(string shipName, string activeSave)
    //{
    //    return GetShipSave(
    //        Path.Combine(Paths.Player));
    //}

    public ShipBuildSave GetShipSave(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Ship save not found: " + path);
            return new ShipBuildSave();
        }

        string json = File.ReadAllText(path);

        ShipBuildSave shipSave = JsonUtility.FromJson<ShipBuildSave>(json);

        if (shipSave.partList == null)
            return new ShipBuildSave();

        return shipSave;
    }

    #endregion

    ShipBuildSave ConstructShipBuildSave(string shipName, IEnumerable<EditorShipPart> parts)
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

    ShipGameSave ConstructShipGameSave(string shipName, IEnumerable<EditorShipPart> parts)
    {
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
}
