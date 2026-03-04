using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ShipPresetManager : MonoBehaviour
{
    [SerializeField] ScrollMenuManager scrollMenu;
    private List<RectTransform> presetItems;

    public void DisplayPresets()
    {
        LoadPresetItems();

        scrollMenu.ShowPageOne(presetItems);
    }

    void LoadPresetItems()
    {
        presetItems = new();

        string dataPath = Paths.ShipPresetDataPath;
        string spritesPath = Paths.ShipPresetSpritesPath;

        if (!Directory.Exists(dataPath) || !Directory.Exists(spritesPath))
            return;

        string[] dataFiles = Directory.GetFiles(dataPath, "*.json", SearchOption.TopDirectoryOnly);
        string[] spriteFiles = Directory.GetFiles(spritesPath, "*.png", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < dataFiles.Length; i++)
        {
            // dont need to load all this data for each ship, just name and sprite at this stage
            //string json = File.ReadAllText(dataFiles[i]);
            //var data = JsonUtility.FromJson<ShipSave>(json);
            string name = Path.GetFileNameWithoutExtension(dataFiles[i]);

            byte[] spriteBytes = File.ReadAllBytes(spriteFiles[i]);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(spriteBytes); // auto-resizes so 2,2 doesnt matter
            texture.filterMode = FilterMode.Point;

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );


            RectTransform obj = Instantiate(Assets.i.uiShipPrefab);
            var uiShip = obj.GetComponent<UIShip>();
            uiShip.Init(sprite, name);
            presetItems.Add(obj);
            obj.gameObject.SetActive(false);
        }
    }

    public void ScrollUp()
    {
        scrollMenu.ScrollUp(presetItems);
    }

    public void ScrollDown()
    {
        scrollMenu.ScrollDown(presetItems);
    }
}
