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
        var shipSave = ConstructShipSave(shipData, parts);

        PlayerSaveManager.LoadSaveNames();

        if(PlayerSaveManager.SaveNames.Contains(shipSave.shipName))
        {
            for(int i = 0; i < playerSave.shipBuilds.Count; i++)
            {
                ShipSave existingShip = playerSave.shipBuilds[i];
                if(existingShip.shipName == shipSave.shipName)
                {
                    existingShip = shipSave;
                }
            }
        }
        else
        {
            playerSave.shipBuilds.Add(shipSave);
        }

        PlayerSaveManager.SaveToJson(playerSave); // maybe not needed here, needs to happen at some point though
    }

    private void SaveShipPresetTexture(UIShipData shipData, bool dev = false)
    {
        byte[] pngBytes = shipData.shipSprite.texture.EncodeToPNG();

        string path = dev ? Paths.DevPresetSpritesPath : Paths.PlayerPresetSpritesPath;

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
