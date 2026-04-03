using System.Collections.Generic;
using System.IO;
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

        SaveShipBuildData(shipData, parts);

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
        var shipSave = ConstructShipSave(shipData, parts);

        string json = JsonUtility.ToJson(shipSave, true);

        string path = dev ? Paths.DevPresetDataPath : Paths.PlayerPresetDataPath;

        File.WriteAllText(
            Path.Combine(path, $"{shipData.shipName}.json"), 
            json );
    }

    public void SaveShipBuildData(UIShipData shipData, IEnumerable<EditorShipPart> parts)
    {
        var shipSave = ConstructShipSave(shipData, parts);

        string json = JsonUtility.ToJson(shipSave, true);

        string path = Paths.ShipBuildDataPath(PlayerSaveManager.ActiveSave.saveName);

        Directory.CreateDirectory(path);

        File.WriteAllText(
            Path.Combine(path, $"{shipData.shipName}.json"),
            json);
    }

    public ShipSave GetShipPreset(string presetName, bool dev = false)
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

    public ShipSave GetShipSave(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Ship save not found: " + path);
            return new ShipSave();
        }

        string json = File.ReadAllText(path);

        ShipSave shipSave = JsonUtility.FromJson<ShipSave>(json);

        if (shipSave.partList == null)
            return new ShipSave();

        return shipSave;
    }

    #endregion

    ShipSave ConstructShipSave(UIShipData shipData, IEnumerable<EditorShipPart> parts)
    {
        ShipSave shipSave = new ShipSave
        {
            shipName = shipData.shipName,
            partList = new List<PartStruct>()
        };

        foreach (var part in parts)
        {
            shipSave.partList.Add(new PartStruct
            {
                partName = part.partData.name,
                xPos = part.position.x,
                yPos = part.position.y,
                xFlipped = part.xFlipped,
                yFlipped = part.yFlipped,
                rotation = Mathf.RoundToInt(part.Rotation) % 360
            });
        }

        return shipSave;
    }
}
