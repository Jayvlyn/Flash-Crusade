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
    [SerializeField] NavItem saveAsPresetButton;

    List<RectTransform> presetItems;
    UIShipData currentBuildData;

    HashSet<string> devPresetNames;
    public bool DevPresetNameExists(string name) => devPresetNames.Contains(name);

    HashSet<string> playerPresetNames;
    public bool PlayerPresetNameExists(string name) => playerPresetNames.Contains(name);


    public void DisplayPresets(UIShipData currentBuildData)
    {
        this.currentBuildData = currentBuildData;
        SetCurrentBuildAsPreview();

        LoadAllPresetItems();
        scrollMenu.ShowPageOne(presetItems);
    }

    public void DisplayPresets()
    {
        currentBuildData.shipSprite = null;
        currentBuildData.shipName = "";
        ShowDefaultPreview();

        LoadAllPresetItems();
        scrollMenu.ShowPageOne(presetItems);
    }

    void LoadAllPresetItems()
    {
        if(presetItems != null)
        {
            foreach (var preset in presetItems)
            {
                Destroy(preset.gameObject);
            }
        }

        presetItems = new();

        string dataPath = Paths.DevPresetDataPath;
        string spritesPath = Paths.DevPresetSpritesPath;
        devPresetNames = new();

        LoadPresetItems(dataPath, spritesPath, devPresetNames);

        dataPath = Paths.PlayerPresetDataPath;
        spritesPath = Paths.PlayerPresetSpritesPath;
        playerPresetNames = new();

        LoadPresetItems(dataPath, spritesPath, playerPresetNames);

    }

    void LoadPresetItems(string dataPath, string spritesPath, HashSet<string> names)
    {
        if (!Directory.Exists(dataPath) || !Directory.Exists(spritesPath))
            return;

        string[] dataFiles = Directory.GetFiles(dataPath, "*.json", SearchOption.TopDirectoryOnly);
        string[] spriteFiles = Directory.GetFiles(spritesPath, "*.png", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < dataFiles.Length; i++)
        {
            string name = Path.GetFileNameWithoutExtension(dataFiles[i]);
            names.Add(name);

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

    public void OnSaveBuildAsPresetSelected()
    {
        EventBus.Publish(new SavePresetEvent());
    }

    [MenuItem("Tools/Save Dev Preset")]
    public static void OnSaveDevPreset()
    {
        EventBus.Publish(new SaveDevPresetEvent());
    }

    public void OnPresetSelected()
    {
        if (hoveredUIShip == null) return;
        EventBus.Publish(new PresetSelectedEvent { 
            presetName = hoveredUIShip.ShipName 
        });
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

    public void SetCurrentBuildAsPreview()
    {
        SetPresetPreview(currentBuildData.shipName, currentBuildData.shipSprite);
        saveAsPresetButton.Disabled = false;
    }

    public void ShowDefaultPreview()
    {
        SetPresetPreview("Invalid Build", Assets.i.shipSilhouette);
        saveAsPresetButton.Disabled = true;
    }

    public void ShowBuildOrDefaultPreview()
    {
        if (currentBuildData.shipSprite != null)
            SetCurrentBuildAsPreview();
        else
            ShowDefaultPreview();
    }
}
