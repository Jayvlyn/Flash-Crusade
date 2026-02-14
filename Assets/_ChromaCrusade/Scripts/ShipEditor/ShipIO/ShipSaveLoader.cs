using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShipSaveLoader
{
    static string MainPath => Application.persistentDataPath;
    static string ShipSpritePath => Path.Combine(MainPath, "ShipSprites");
    static string ShipDataPath => Path.Combine(MainPath, "ShipData");

    BuildArea buildArea;

    #region Public API

    public ShipSaveLoader(BuildArea buildArea)
    {
        this.buildArea = buildArea;

        Directory.CreateDirectory(ShipSpritePath);
        Directory.CreateDirectory(ShipDataPath);
    }

    public void SaveCurrentBuild(string shipName)
    {
        // Save Sprite
        PartSpriteCombiner spriteCombiner = new PartSpriteCombiner(buildArea);
        Texture2D shipTexture = spriteCombiner.CreateCombinedTexture();
        SaveShipTexture(shipTexture, shipName);

        // Save Build Data
        var shipSave = new ShipSave
        {
            shipName = shipName,
            partList = new List<PartStruct>()
        };

        foreach(var part in buildArea.Parts)
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
            Path.Combine(ShipDataPath, $"{shipName}.json"),
            json
        );
    }

    public void LoadBuild(string shipName)
    {
        string path = Path.Combine(ShipDataPath, $"{shipName}.json");

        if (!File.Exists(path))
        {
            Debug.LogError("Ship save not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);

        ShipSave shipSave = JsonUtility.FromJson<ShipSave>(json);

        if (shipSave.partList == null)
            return;

        Debug.Log("unfinished");
    }

    #endregion

    private void SaveShipTexture(Texture2D texture, string shipName)
    {
        byte[] pngBytes = texture.EncodeToPNG();

        string path = Path.Combine(ShipSpritePath, $"{shipName}.png");

        File.WriteAllBytes(path, pngBytes);

        Object.Destroy(texture);
    }
}
