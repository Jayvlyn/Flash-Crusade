using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShipSaveLoader
{
    IEnumerable<ShipPart> parts;

    #region Public API

    public ShipSaveLoader(IEnumerable<ShipPart> parts)
    {
        this.parts = parts;

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

    public void SaveBuildAsPreset(UIShipData shipData)
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
                posX = part.position.x,
                posY = part.position.y,
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

    //public void LoadBuild(string shipName)
    //{
    //    string path = Path.Combine(Paths.ShipDataPath, $"{shipName}.json");

    //    if (!File.Exists(path))
    //    {
    //        Debug.LogError("Ship save not found: " + path);
    //        return;
    //    }

    //    string json = File.ReadAllText(path);

    //    ShipSave shipSave = JsonUtility.FromJson<ShipSave>(json);

    //    if (shipSave.partList == null)
    //        return;

    //    Debug.Log("unfinished");
    //}

    public void LoadPreset(string presetName)
    {

    }

    #endregion
}
