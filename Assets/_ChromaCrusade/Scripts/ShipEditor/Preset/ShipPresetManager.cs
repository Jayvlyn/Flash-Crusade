using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ShipPresetManager : MonoBehaviour
{
    [SerializeField] ScrollMenuManager scrollMenu;
    private List<ShipPresetItem> presetItems;

    private void LoadPresets()
    {
        presetItems = new();

        string dataPath = Paths.ShipPresetDataPath;
        string spritesPath = Paths.ShipPresetSpritesPath;

        if (!Directory.Exists(dataPath) || !Directory.Exists(spritesPath))
            return;

        string[] dataFiles = Directory.GetFiles(dataPath, "*.json", SearchOption.TopDirectoryOnly);
        string[] spriteFiles = Directory.GetFiles(spritesPath, "*.png", SearchOption.TopDirectoryOnly);

        for(int i = 0; i < dataFiles.Length; i++)
        {
            string json = File.ReadAllText(dataFiles[i]);

            var data = JsonUtility.FromJson<ShipSave>(json);

            byte[] spriteBytes = File.ReadAllBytes(spriteFiles[i]);


            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(spriteBytes); // auto-resizes so 2,2 doesnt matter

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            var item = new ShipPresetItem();
        }

    }
}
