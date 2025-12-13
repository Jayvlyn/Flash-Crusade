using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartOrganizer : MonoBehaviour
{
    [Header("Refs")]
    public NavManager nav;
    public RectTransform itemPanel;
    public RectTransform defaultPartSpawn;
    public NavItem[] partSelectors;
    private PartCounter[] partCounters;
    private List<EditorShipPart> shownParts;

    private List<InventoryEntry> cabins = new();
    private List<InventoryEntry> cores = new();
    private List<InventoryEntry> wings = new();
    private List<InventoryEntry> weapons = new();
    private List<InventoryEntry> utilities = new();
    [System.Serializable]
    public class InventoryEntry
    {
        public ShipPartData data;
        public int count;
    }

    private PartType showState;
    private int pageCount;
    private int currentPage = 1;

    private void Awake()
    {
        Init();
    }

    #region Initialization

    private void Init()
    {
        shownParts = new List<EditorShipPart>();

        PartInventory inv = InventoryLoader.Load();

        partCounters = new PartCounter[partSelectors.Length];
        for (int i = 0; i < partSelectors.Length; i++)
        {
            partCounters[i] = partSelectors[i].rect.GetChild(0).GetComponent<PartCounter>();
        }

        cabins = Resolve(inv.cabins, PartType.Cabin);
        cores = Resolve(inv.cores, PartType.Core);
        wings = Resolve(inv.wings, PartType.Wing);
        weapons = Resolve(inv.weapons, PartType.Weapon);
        utilities = Resolve(inv.utilities, PartType.Utility);
    }

    private List<InventoryEntry> Resolve(List<PartStack> stackList, PartType type)
    {
        var resolved = new List<InventoryEntry>();

        foreach (var stack in stackList)
        {
            ShipPartData data = PartDatabase.Instance.Get(stack.name);

            if (data == null)
            {
                Debug.LogWarning($"Inventory references missing part: {stack.name}");
                continue;
            }

            resolved.Add(new InventoryEntry { data = data, count = stack.count });
        }

        return resolved;
    }

    #endregion

    #region Public API

    public bool TryTakePart(ShipPartData data, out EditorShipPart createdPart)
    {
        createdPart = null;

        var list = GetListForType(data.PartType);
        var entry = list.Find(e => e.data == data);

        if (entry == null || entry.count <= 0)
            return false;

        entry.count--;

        if (entry.count == 0)
            list.Remove(entry);

        GameObject obj = Instantiate(Assets.i.editorShipPartPrefab);
        var part = obj.GetComponent<EditorShipPart>();
        part.Init(data);

        createdPart = part;

        RefreshCurrentPage();
        return true;
    }

    public void AddPart(ShipPartData data)
    {
        List<InventoryEntry> list = GetListForType(data.PartType);

        InventoryEntry entry = list.Find(e => e.data == data);

        if (entry != null)
        {
            entry.count++;
        }
        else
        {
            entry = new InventoryEntry { data = data, count = 1 };
            list.Add(entry);
        }

        UpdatePageCount(list.Count, partSelectors.Length);

        RefreshCurrentPage();
    }

    public void SetPartToDefaultStart(EditorShipPart part)
    {
        part.rect.SetParent(defaultPartSpawn.parent, worldPositionStays: false);
        part.rtf.target = defaultPartSpawn;
        part.rtf.stretch = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(defaultPartSpawn);
        part.rtf.enabled = true;
        part.rtf.Follow();
    }

    #endregion

    #region Paging

    private void ChangeShowState(PartType showState)
    {
        this.showState = showState;
        switch (showState)
        {
            case PartType.Cabin:
                ShowParts(cabins);
                break;
            case PartType.Core:
                ShowParts(cores);
                break;
            case PartType.Wing:
                ShowParts(wings);
                break;
            case PartType.Weapon:
                ShowParts(weapons);
                break;
            case PartType.Utility:
                ShowParts(utilities);
                break;
        }
    }

    private void ClearParts()
    {
        foreach (var part in shownParts)
            Destroy(part.gameObject);
        shownParts.Clear();
    }

    private void ShowParts(List<InventoryEntry> partArray)
    {
        UpdatePageCount(partArray.Count, partSelectors.Length);

        ClearParts();

        int elementsPerPage = partSelectors.Length;
        int startIndex = (currentPage - 1) * elementsPerPage;
        int endIndex = Mathf.Min(startIndex + elementsPerPage, partArray.Count);

        int selectorIndex = 0;

        for (int i = startIndex; i < endIndex; i++)
        {
            var inventoryEntry = partArray[i];

            NavItem partSelector = partSelectors[selectorIndex];
            partSelector.onSelected.RemoveAllListeners();

            GameObject obj = Instantiate(Assets.i.editorShipPartPrefab, itemPanel);
            
            EditorShipPart part = obj.GetComponent<EditorShipPart>();
            part.Init(inventoryEntry.data);
            part.rtf.enabled = true;
            part.rtf.target = partSelector.rect;

            shownParts.Add(part);

            partCounters[selectorIndex].SetCount(inventoryEntry.count);

            partSelector.onSelected.AddListener(() =>
            {
                nav.OnInventoryPartGrabbed(inventoryEntry.data);
            });

            selectorIndex++;
        }

        for (int i = selectorIndex; i < partCounters.Length; i++)
            partCounters[i].SetCount(0);
    }

    private void UpdatePageCount(int totalElements, int elementsPerPage = 18)
    {
        currentPage = 1;
        pageCount = Mathf.CeilToInt((float)totalElements / elementsPerPage);
    }

    public void RefreshCurrentPage()
    {
        switch (showState)
        {
            case PartType.Cabin: ShowParts(cabins); break;
            case PartType.Core: ShowParts(cores); break;
            case PartType.Wing: ShowParts(wings); break;
            case PartType.Weapon: ShowParts(weapons); break;
            case PartType.Utility: ShowParts(utilities); break;
        }
    }

    private List<InventoryEntry> GetListForType(PartType type)
    {
        return type switch
        {
            PartType.Cabin => cabins,
            PartType.Core => cores,
            PartType.Wing => wings,
            PartType.Weapon => weapons,
            PartType.Utility => utilities,
            _ => null
        };
    }

    #endregion

    #region Event Responses

    public void OnCabinTabSelected()
    {
        ChangeShowState(PartType.Cabin);
    }

    public void OnCoreTabSelected()
    {
        ChangeShowState(PartType.Core);
    }

    public void OnWingTabSelected()
    {
        ChangeShowState(PartType.Wing);
    }

    public void OnWeaponTabSelected()
    {
        ChangeShowState(PartType.Weapon);
    }

    public void OnUtilityTabSelected()
    {
        ChangeShowState(PartType.Utility);
    }

    public void OnUpSelected()
    {
        if (currentPage > 1)
        {
            currentPage--;
            RefreshCurrentPage();
        }
    }

    public void OnDownSelected()
    {
        if (currentPage < pageCount)
        {
            currentPage++;
            RefreshCurrentPage();
        }
    }

    #endregion
}
