using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShipSaveLoader
{
    #region Public API

    public ShipSaveLoader()
    {
        Directory.CreateDirectory(Paths.ShipPresetSpritesPath);
        Directory.CreateDirectory(Paths.ShipPresetDataPath);
    }

    private void SaveShipPresetTexture(UIShipData shipData)
    {
        byte[] pngBytes = shipData.shipSprite.texture.EncodeToPNG();

        string path = Path.Combine(Paths.ShipPresetSpritesPath, $"{shipData.shipName}.png");

        File.WriteAllBytes(path, pngBytes);

        //Object.Destroy(shipData.shipSprite.texture);
    }

    public void SaveBuildAsPreset(UIShipData shipData, IEnumerable<ShipPart> parts)
    {
        SaveShipPresetTexture(shipData);

        // Save Build Data
        var shipSave = new ShipSave
        {
            shipName = shipData.shipName,
            partList = new List<PartStruct>()
        };

        foreach(var part in parts)
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

        string json = JsonUtility.ToJson(shipSave, true);

        File.WriteAllText(
            Path.Combine(Paths.ShipPresetDataPath, $"{shipData.shipName}.json"),
            json
        );
    }

    public ShipSave GetShipBuild(string shipName, string activeSave)
    {
        return GetShipSave(
            Path.Combine(Paths.ShipBuildDataPath(activeSave), 
            $"{shipName}.json"));
    }

    public ShipSave GetShipPreset(string presetName)
    {
        return GetShipSave(
            Path.Combine(Paths.ShipPresetDataPath, 
            $"{presetName}.json"));
    }

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
}
