using System.IO;
using UnityEditor;
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
        PartSpriteCombiner psc = new PartSpriteCombiner(buildArea);
        Texture2D shipTexture = psc.CreateCombinedTexture();
        SaveShipTexture(shipTexture, shipName);
    }

    #endregion

    private void SaveShipTexture(Texture2D texture, string shipName)
    {
        byte[] pngBytes = texture.EncodeToPNG();

        string path = Path.Combine(ShipSpritePath, $"{shipName}.png");

        File.WriteAllBytes(path, pngBytes);
    }
}
