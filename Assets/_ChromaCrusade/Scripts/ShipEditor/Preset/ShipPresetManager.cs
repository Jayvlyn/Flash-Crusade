using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ShipPresetManager : MonoBehaviour
{
    [SerializeField] ScrollMenuManager scrollMenu;
    [SerializeField] Image previewImage;
    [SerializeField] TMP_Text previewText;

    List<RectTransform> presetItems;
    UIShipData currentBuildData;

    public void DisplayPresets(UIShipData currentBuildData)
    {
        this.currentBuildData = currentBuildData;
        SetPresetPreview(currentBuildData.shipName, currentBuildData.shipSprite);

        LoadPresetItems();
        scrollMenu.ShowPageOne(presetItems);
    }

    public void DisplayPresets()
    {
        LoadPresetItems();
        scrollMenu.ShowPageOne(presetItems);

        //ShowDefaultPreview();
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

    //public void LoadPresetAsBuild(string shipName)
    //{
    //    string path = Path.Combine(Paths.ShipPresetDataPath, $"{shipName}.json");

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

    public void OnPresetSelected()
    {
        Debug.Log($"Selected {hoveredUIShip.ShipName}");
        // send event with the ship name? instead below because we need editormanager for all that shi
        //LoadPresetAsBuild(hoveredUIShip.shipName);
    }

    UIShip hoveredUIShip;
    public void OnPresetHovered(int hoveredIndex)
    {
        var (start, end) = scrollMenu.Pager.GetRange(presetItems.Count, scrollMenu.ElementsPerPage);

        int i = start + hoveredIndex;
        if (i >= presetItems.Count)
        {
            ShowBuildOrDefaultPreview();
        }
        else
        {
            RectTransform hoveredPreset = presetItems[i];

            hoveredUIShip = hoveredPreset.GetComponent<UIShip>();

            SetPresetPreview(hoveredUIShip.ShipName, hoveredUIShip.ShipSprite);
        }

    }

    public void SetPresetPreview(string name, Sprite sprite)
    {
        previewImage.sprite = sprite;
        previewText.text = name;
    }

    public void ShowDefaultPreview() => SetPresetPreview("Invalid Build", Assets.i.shipSilhouette);

    public void ShowBuildOrDefaultPreview()
    {
        if (currentBuildData.shipSprite != null)
            SetPresetPreview(currentBuildData.shipName, currentBuildData.shipSprite);
        else
            ShowDefaultPreview();
    }
}
