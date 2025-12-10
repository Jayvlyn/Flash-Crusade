using System.Collections.Generic;
using UnityEngine;

public class PartOrganizer : MonoBehaviour
{
    public NavManager nav;

    public NavItem[] partSelectors;
    public PartCounter[] partCounters;

    public List<InventoryEntry> cabins = new();
    public List<InventoryEntry> cores = new();
    public List<InventoryEntry> wings = new();
    public List<InventoryEntry> weapons = new();
    public List<InventoryEntry> utilities = new();

    [System.Serializable]
    public class InventoryEntry
    {
        public ShipPartData data;
        public int count;
    }

    public RectTransform itemPanel;

    public PartType showState;

    public List<EditorShipPart> shownParts;

    private int pageCount;
    private int currentPage = 1;

    private void Awake()
    {
        shownParts = new List<EditorShipPart>();

        PartInventory inv = InventoryLoader.Load();

        partCounters = new PartCounter[partSelectors.Length];
        for(int i = 0; i < partSelectors.Length; i++)
        {
            partCounters[i] = partSelectors[i].rect.GetChild(0).GetComponent<PartCounter>();
        }

        cabins = Resolve(inv.cabins, PartType.Cabin);
        cores = Resolve(inv.cores, PartType.Core);
        wings = Resolve(inv.wings, PartType.Wing);
        weapons = Resolve(inv.weapons, PartType.Weapon);
        utilities = Resolve(inv.utilities, PartType.Utility);
    }

    private void ChangeShowState(PartType showState)
    {
        this.showState = showState;
        switch (showState)
        {
            case PartType.Cabin:
                UpdatePageCount(cabins.Count, partSelectors.Length);
                ShowParts(cabins);
                break;
            case PartType.Core:
                UpdatePageCount(cores.Count, partSelectors.Length);
                ShowParts(cores);
                break;
            case PartType.Wing:
                UpdatePageCount(wings.Count, partSelectors.Length);
                ShowParts(wings);
                break;
            case PartType.Weapon:
                UpdatePageCount(weapons.Count, partSelectors.Length);
                ShowParts(weapons);
                break;
            case PartType.Utility:
                UpdatePageCount(utilities.Count, partSelectors.Length);
                ShowParts(utilities);
                break;
        }
        currentPage = 1;
    }

    private void ClearParts()
    {
        foreach (var part in shownParts)
            Destroy(part.gameObject);
        shownParts.Clear();
    }

    private void ShowParts(List<InventoryEntry> partArray)
    {
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
            part.rtf.target = partSelector.rect;
            part.rtf.enabled = true;

            shownParts.Add(part);

            partCounters[selectorIndex].SetCount(inventoryEntry.count);

            partSelector.onSelected.AddListener(() =>
            {
                TakePart(inventoryEntry);

                GameObject clone = Instantiate(Assets.i.editorShipPartPrefab, itemPanel);
                EditorShipPart runtimePart = clone.GetComponent<EditorShipPart>();
                runtimePart.Init(inventoryEntry.data);

                runtimePart.rtf.target = partSelector.rect;
                part.rtf.enabled = true;

                nav.OnInventoryPartGrabbed(runtimePart, this);
            });

            selectorIndex++;
        }

        for (int i = selectorIndex; i < partCounters.Length; i++)
            partCounters[i].SetCount(0);
    }


    private void UpdatePageCount(int totalElements, int elementsPerPage = 18)
    {
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

    public void TakePart(InventoryEntry entry)
    {
        entry.count--;

        if (entry.count <= 0)
        {
            RemoveEntry(entry);
        }

        RefreshCurrentPage();
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

    private void RemoveEntry(InventoryEntry entry)
    {
        switch (showState)
        {
            case PartType.Cabin: cabins.Remove(entry); break;
            case PartType.Core: cores.Remove(entry); break;
            case PartType.Wing: wings.Remove(entry); break;
            case PartType.Weapon: weapons.Remove(entry); break;
            case PartType.Utility: utilities.Remove(entry); break;
        }
        UpdatePageCount(cabins.Count, partSelectors.Length);
    }

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
