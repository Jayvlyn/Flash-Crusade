#if UNITY_EDITOR
using NaughtyAttributes;
using OdinSerializer.Utilities;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ImporterPart : MonoBehaviour
{
    #region Data
    public enum PartType 
    { 
        Select = 0, 
        Cabin = 1, 
        Core = 2,
        Wing = 3, 
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
    int mass = 1;
    public int priceAddon = 0;

    [Header("Weapon Attributes")]
    [ShowIf(nameof(IsWeapon))] public int damage = 10;
    [ShowIf(nameof(IsWeapon))] public float spread = 1;
    [ShowIf(nameof(IsWeapon))] public float fireRate = 1;
    [ShowIf(nameof(IsWeapon))] public FireType fireType = FireType.Select;
    public enum FireType
    {
        Select = 0,
        Projectile = 1,
        Beam = 2,
        Wave = 3,
    }
    [ShowIf(nameof(IsWeapon))] public List<FirePoint> firePoints;
    List<ImporterFirepoint> importerFirepoints;

    bool IsProjectile() => fireType == FireType.Projectile && partType == PartType.Weapon;
    bool IsBeam() => fireType == FireType.Beam && partType == PartType.Weapon;
    bool IsWave() => fireType == FireType.Wave && partType == PartType.Weapon;

    // Projectile Weapon Data
    [ShowIf(nameof(IsProjectile))]
    public ProjectileData projectile;

    [ShowIf(nameof(IsBeam))]
    public float beamThickness = 3f;
    [ShowIf(nameof(IsBeam))]
    public float chargeTime = 0.5f;

    [ShowIf(nameof(IsWave))]
    public float growSpeed = 1;

    [Header("Energy Core Attributes")]
    [ShowIf(nameof(IsCore))] public int energy = 100;

    [Header("Wing Attributes")]
    [ShowIf(nameof(IsWing))] public int mobility = 1;

    [Header("Pilot Cabin Attributes")]
    [ShowIf(nameof(IsCabin))] public int handling = 1;

    [Header("Utility Attributes")]
    [ShowIf(nameof(IsUtility))] public UtilityType utilityType;
    public enum UtilityType 
    { 
        Select = 0,
        Dock = 1,       // Other ships can dock here, more options when piloting ship. "space station"
        Enhancer = 2,   // Enhances connected weapons
        Capacitor = 3,    // Recharges energy passively
    }

    private static readonly Dictionary<PartType, string> FolderNames = new()
    {
        { PartType.Cabin, "Cabins" },
        { PartType.Core, "Cores" },
        { PartType.Wing, "Wings" },
        { PartType.Weapon, "Weapons" },
        { PartType.Utility, "Utilities" }
    };

    private static readonly Dictionary<PartType, System.Type> SoTypeMap = new()
    {
        { PartType.Cabin, typeof(ShipCabinData) },
        { PartType.Core, typeof(ShipCoreData) },
        { PartType.Wing, typeof(ShipWingData) },
        { PartType.Weapon, typeof(ShipWeaponData) },
        { PartType.Utility, typeof(ShipUtilityData) }
    };

    #endregion

    #region References
    [HideInInspector] public Image image;
    [HideInInspector] public ImporterSegment[] segments;
    [HideInInspector] public GameObject segmentButtonsParent;
    [HideInInspector] public RectTransform firepointPrefab;
    [HideInInspector] public Transform firepointsParent;
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
    private bool ResetClicked() => resetClicked == true;
    private bool ResetNotClicked() => resetClicked == false;
    private bool AddingFirepoint() => addingFirepoint == true;
    private bool ShouldShowAddFirepoint() => !AddingFirepoint() && IsWeapon();
    private bool ShouldReadForFirepointPos() => addingFirepoint && !recievedPosition && Input.GetMouseButtonDown(0);

    private bool ShowSavePartsButton() => !addingFirepoint;
    private bool ShowResetButton() => !addingFirepoint && ResetNotClicked();

    private bool ShowShowSegmentsButton() => segmentsShown == false && !addingFirepoint;
    private bool ShowHideSegmentsButton() => segmentsShown == true && !addingFirepoint;
    private bool ShowShowFirepointsButton() => firepointsShown == false && firePoints != null && firePoints.Count > 0 && !addingFirepoint;
    private bool ShowHideFirepointsButton() => firepointsShown == true && firePoints != null && firePoints.Count > 0 && !addingFirepoint;

    private bool segmentsShown = true;
    private bool firepointsShown = false;

    //private bool IsProjectileWeapon() => weapon
    #endregion

    [ShowIf(nameof(ShowShowFirepointsButton)), Button("Show Firepoints")]
    private void ShowFirepoints()
    {
        firepointsParent.gameObject.SetActive(true);
        firepointsShown = true;
    }

    [ShowIf(nameof(ShowHideFirepointsButton)), Button("Hide Firepoints")]
    private void HideFirepoints()
    {
        firepointsParent.gameObject.SetActive(false);
        firepointsShown = false;
    }

    [ShowIf(nameof(ShowShowSegmentsButton)), Button("Show Segments")]
    private void ShowSegments()
    {
        segmentButtonsParent.SetActive(true);
        segmentsShown = true;
    }

    [ShowIf(nameof(ShowHideSegmentsButton)), Button("Hide Segments")]
    private void HideSegments()
    {
        segmentButtonsParent.SetActive(false);
        segmentsShown = false;
    }

    bool addingFirepoint;
    bool recievedPosition;
    [ShowIf(nameof(ShouldShowAddFirepoint)), Button("Add Firepoint")]
    private void AddFirepoint()
    {
        addingFirepoint = true;
        recievedPosition = false;
        ShowFirepoints();
        HideSegments();
    }

    [ShowIf(nameof(AddingFirepoint)), Button("Cancel Adding Firepoint")]
    private void CancelAddFirepoint()
    {
        addingFirepoint = false;
        recievedPosition = false;

        ShowSegments();
    }

    void PlaceFirepoint(Vector2Int firepointPos, FirePoint firePoint)
    {
        Vector2 pos = FirepointToScreenPos(firepointPos);

        RectTransform fpInstance = Instantiate(firepointPrefab, firepointsParent.transform);
        fpInstance.anchoredPosition = pos;

        ImporterFirepoint fp = fpInstance.GetComponent<ImporterFirepoint>();
        fp.refFirepoint = firePoint;
        fp.Init();

        if (importerFirepoints == null) importerFirepoints = new();
        importerFirepoints.Add(fp);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<FirepointDeletedEvent>(OnFirepointDeleted);
    }


    private void OnDisable()
    {
        EventBus.Unsubscribe<FirepointDeletedEvent>(OnFirepointDeleted);
    }

    void OnFirepointDeleted(FirepointDeletedEvent e)
    {
        firePoints.RemoveAll(fp => fp.position == e.position);
    }

    private void Start()
    {
        if (partSprite != null)
            image.sprite = partSprite;
        else
            image.enabled = false;
    }

    private void Update()
    {
        if (ShouldReadForFirepointPos())
        {
            Vector2 mouse01 = new Vector2(
                Input.mousePosition.x / Screen.width,
                Input.mousePosition.y / Screen.height
            );

            Vector2Int firepointCoord = ScreenToFirepointPos(mouse01);

            if (firepointCoord.x < 0 || firepointCoord.x > 8 || firepointCoord.y < 0 || firepointCoord.y > 8) return;

            foreach(var firepoint in firePoints)
                if (firepoint.position == firepointCoord) return; // fp already exists here.
           
            recievedPosition = true;

            FirePoint fp = new FirePoint
            {
                position = firepointCoord,
                fireColor = Color.white,
                fireDirection = FireDirection.North
            };

            if (firePoints == null) firePoints = new();
            firePoints.Add(fp);

            PlaceFirepoint(firepointCoord, fp);

            CancelAddFirepoint();
        }
    }

    Vector2Int ScreenToFirepointPos(Vector2 pos) // pos should be 0-1
    {
        Vector2 posInSquare = new Vector2(pos.x, 1 - pos.y);

        Vector2Int firepointCoord = new Vector2Int(
            (int)(posInSquare.x * 9),
            (int)(posInSquare.y * 9));

        return firepointCoord;
    }

    Vector2 FirepointToScreenPos(Vector2Int pos)
    {
        Vector2 screenPos = new Vector3(
            pos.x / 9f ,
            pos.y / 9f);

        return new Vector2(
            screenPos.x * Screen.width + 25, 
            (1-screenPos.y) * Screen.height - 25);
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
        {
            image.enabled = false;
        }
    }

    //[HorizontalLine(10, EColor.Green), ReadOnly]
    //public string buttons = "";
    [ShowIf(nameof(ShowSavePartsButton)), Button("Save Part")]
    private void SavePart()
    {
        if(EmptyName())
        {


            //Debug.LogError("Cannot create ScriptableObject: partName is empty.");
            //return;
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

        if(partType == PartType.Weapon && (firePoints == null || firePoints.Count <= 0))
        {
            Debug.LogError("Cannot create ScriptableObject: Weapons must have at least one fire point.");
            return;
        }

        string folder = PartListGenerator.PartsRootFolder +"/"+ FolderNames[partType];
        string assetPath = $"{folder}/{partName}.asset";

        if (!AssetDatabase.IsValidFolder(folder.TrimEnd('/')))
        {
            Debug.LogError(folder.TrimEnd('/'));
            return;
        }

        var existing = AssetDatabase.LoadAssetAtPath<ShipPartData>(assetPath);
        if (existing != null)
        {
            Debug.LogWarning($"Overwriting existing asset at {assetPath}");
            //existing.Apply(this);
            ApplyStats(existing);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
        }

        var so = ScriptableObject.CreateInstance(SoTypeMap[partType]) as ShipPartData;
        //so.Apply(this);
        ApplyStats(so);
        AssetDatabase.CreateAsset(so, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(so); // highlight in editor

        Debug.Log($"Created ScriptableObject: {assetPath}");
    }


    [ShowIf(nameof(ResetClicked)), Button("CANCEL Reset")]
    private void CancelReset()
    {
        resetClicked = false;
    }

    [ShowIf(nameof(ResetClicked)),Button("CONFIRM Reset")]
    private void ClearFields()
    {
        partSprite = null;
        OnSpriteChangedCallback();
        partType = PartType.Select;
        partName = "";
        mass = 1;
        damage = 10;
        spread = 1;
        fireRate = 1;
        energy = 100;
        mobility = 1;
        handling = 1;
        utilityType = UtilityType.Select;
        foreach(var segment in segments)
        {
            segment.Disable();
        }
        resetClicked = false;
        EventBus.Publish(new ClearImporterEvent());
    }

    bool resetClicked;
    [ShowIf(nameof(ShowResetButton)),Button("Reset")]
    private void ResetButton()
    {
        resetClicked = true;
    }

    private void ApplyStats(ShipPartData partData)
    {
        partData.sprite = partSprite;
        CheckMass();
        partData.mass = mass;
        int price = mass * 5;

        partData.segments = new PartSegment[segments.Length];
        for (int i = 0; i < segments.Length; i++)
        {
            var source = segments[i];
            var seg = new PartSegment
            {
                segmentState = source.segmentState,
                topConnection = new PartConnection { connectionState = source.topConnection.connectionState },
                leftConnection = new PartConnection { connectionState = source.leftConnection.connectionState },
                rightConnection = new PartConnection { connectionState = source.rightConnection.connectionState },
                bottomConnection = new PartConnection { connectionState = source.bottomConnection.connectionState }
            };

            partData.segments[i] = seg;
        }

        if (partData is ShipWingData shipWingData)
        {
            shipWingData.mobility = mobility;
            price += mobility * 10;
        }
        else if (partData is ShipCabinData shipCabinData)
        {
            shipCabinData.handling = handling;
            price += handling * 10;
        }
        else if (partData is ShipCoreData shipCoreData)
        {
            shipCoreData.energy = energy;
            price += energy * 10;
        }
        else if (partData is ShipWeaponData shipWeaponData)
        {
            shipWeaponData.damage = damage;
            shipWeaponData.spread = spread;
            shipWeaponData.fireRate = fireRate;

            shipWeaponData.fireType = (ShipWeaponData.FireType)(int)fireType;

            shipWeaponData.firePoints = firePoints.ToArray();

            if(IsProjectile())
            {
                shipWeaponData.projectile = projectile;
            }
            else if(IsBeam())
            {
                shipWeaponData.beamThickness = beamThickness;
                shipWeaponData.chargeTime = chargeTime;
            }
            else if(IsWave())
            {
                shipWeaponData.growSpeed = growSpeed;
            }

            price += damage * 5;
            price += Mathf.RoundToInt(fireRate * 5);
            price += firePoints.Count * 10;
        }
        else if (partData is ShipUtilityData shipUtilityData)
        {
            shipUtilityData.utilityType = (ShipUtilityData.UtilityType)(int)utilityType;
        }

        partData.price = price + priceAddon;
    }

    /// <summary>
    /// Mass will be set to the number of non-transparent pixels on the sprite
    /// </summary>
    private void CheckMass()
    {
        if (image == null || image.sprite == null)
        {
            mass = 0;
            return;
        }

        Sprite sprite = image.sprite;
        Texture2D tex = sprite.texture;

        Rect rect = sprite.textureRect;

        int xMin = Mathf.FloorToInt(rect.x);
        int yMin = Mathf.FloorToInt(rect.y);
        int width = Mathf.FloorToInt(rect.width);
        int height = Mathf.FloorToInt(rect.height);

        Color[] pixels = tex.GetPixels(xMin, yMin, width, height);

        int count = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a > 0.1f)
                count++;
        }

        mass = count;

        Debug.Log(mass);
    }
}

public struct ClearImporterEvent { }
#endif