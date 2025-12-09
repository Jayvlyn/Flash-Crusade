using NaughtyAttributes;
using OdinSerializer.Utilities;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ImporterPart : MonoBehaviour
{
    #region Data
    public enum PartType { Select = 0, 
        Core = 1,
        Wing = 2, 
        Cabin = 3, 
        Weapon = 4, 
        Utility = 5
    }
    public string partName = "";
    //[ShowAssetPreview]
    [OnValueChanged("OnSpriteChangedCallback")]
    [ValidateInput(nameof(SpriteGiven), "Must assign sprite!")]
    public Sprite partSprite;
    [ValidateInput(nameof(TypeSelected), "Must select part type!")]
    public PartType partType = PartType.Select;
    public float mass = 1;
    public int price = 100;

    [Header("Weapon Attributes")]
    [ShowIf(nameof(IsWeapon))] public int damage = 10;
    [ShowIf(nameof(IsWeapon))] public int projectileSpeed = 1;
    [ShowIf(nameof(IsWeapon))] public float fireRate = 1;

    [Header("Energy Core Attributes")]
    [ShowIf(nameof(IsCore))] public int energy = 100;

    [Header("Wing Attributes")]
    [ShowIf(nameof(IsWing))] public int mobility = 1;

    [Header("Pilot Cabin Attributes")]
    [ShowIf(nameof(IsCabin))] public int handling = 1;

    [Header("Utility Attributes")]
    [ShowIf(nameof(IsUtility))] public UtilityType utilityType;
    public enum UtilityType { Select = 0,
        Dock = 1,       // other ships can dock here
        Reflector = 2,  // bullets get reflected back
        Charger = 3,    // Recharges energy passively
        Converter = 4,  // Converts ice to energy
        Repulsor = 4,   // Launches away nearby enemies
        TractorBeam // Pulls in objects/enemies
    }

    private static readonly Dictionary<PartType, string> FolderNames = new()
    {
        { PartType.Core, "Cores" },
        { PartType.Wing, "Wings" },
        { PartType.Cabin, "Cabins" },
        { PartType.Weapon, "Weapons" },
        { PartType.Utility, "Utilities" }
    };

    private static readonly Dictionary<PartType, System.Type> SoTypeMap = new()
{
    { PartType.Core, typeof(ShipCoreData) },
    { PartType.Wing, typeof(ShipWingData) },
    { PartType.Cabin, typeof(ShipCabinData) },
    { PartType.Weapon, typeof(ShipWeaponData) },
    { PartType.Utility, typeof(ShipUtilityData) }
};

    #endregion

    #region References
    [HideInInspector] public Image image;
    [HideInInspector] public ImporterSegment[] segments;
    #endregion

    #region Conditions
    private bool TypeSelected() => partType != PartType.Select;
    private bool IsCore() => partType == PartType.Core;
    private bool IsWing() => partType == PartType.Wing;
    private bool IsWeapon() => partType == PartType.Weapon;
    private bool IsCabin() => partType == PartType.Cabin;
    private bool IsUtility() => partType == PartType.Utility;
    private bool SpriteGiven() => partSprite != null;
    private bool IsValidDamage(int value) => value >= 0;
    private bool EmptyName() => partName.IsNullOrWhitespace();
    #endregion

    private void Start()
    {
        if (partSprite != null)
            image.sprite = partSprite;
        else
            image.enabled = false;
    }

    private void OnSpriteChangedCallback()
    {
        if (image == null) return;

        if (partSprite != null)
        {
            image.sprite = partSprite;
            image.enabled = true;
        }
        else
            image.enabled = false;
    }

    [Button("Save Part")]
    private void SavePart()
    {
        if(EmptyName())
        {
            Debug.LogError("Cannot create ScriptableObject: partName is empty.");
            return;
        }

        if(!SpriteGiven())
        {
            Debug.LogError("Cannot create ScriptableObject: partSprite is empty.");
            return;
        }

        if(!TypeSelected())
        {
            Debug.LogError("Cannot create ScriptableObject: No part type selected.");
            return;
        }

        if(partType == PartType.Utility && utilityType == UtilityType.Select)
        {
            Debug.LogError("Cannot create ScriptableObject: No utility type selected.");
            return;
        }

        string folder = "Assets/_ChromaCrusade/GameData/Parts/" + FolderNames[partType];
        string assetPath = $"{folder}/{partName}.asset";

        if (!AssetDatabase.IsValidFolder(folder.TrimEnd('/')))
        {
            Debug.LogError("Cannot create ScriptableObject: Invalid folder path");
            return;
        }

        var existing = AssetDatabase.LoadAssetAtPath<ShipPartData>(assetPath);
        if (existing != null)
        {
            Debug.LogWarning($"Overwriting existing asset at {assetPath}");
            existing.Apply(this);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
        }

        var so = ScriptableObject.CreateInstance(SoTypeMap[partType]) as ShipPartData;
        so.Apply(this);
        AssetDatabase.CreateAsset(so, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(so); // highlight in editor

        Debug.Log($"Created ScriptableObject: {assetPath}");
    }
}
