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

    private PartType showState;
    private int pageCount;
    private int currentPage = 1;

    private PartInventoryModel partInventory;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        shownParts = new List<EditorShipPart>();

        PartInventory inv = InventoryLoader.Load();
        partInventory = new PartInventoryModel(inv);

        partCounters = new PartCounter[partSelectors.Length];

        for (int i = 0; i < partSelectors.Length; i++)
            partCounters[i] = partSelectors[i].rect.GetChild(0).GetComponent<PartCounter>();   
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

    private void ChangeShowState(PartType showState)
    {
        this.showState = showState;
        RefreshCurrentPage();
    }

    private void ClearParts()
    {
        foreach (var part in shownParts)
            Destroy(part.gameObject);
        shownParts.Clear();
    }

    private void ShowParts(IReadOnlyList<PartInventoryModel.Entry> parts)
    {
        UpdatePageCount(parts.Count, partSelectors.Length);

        ClearParts();

        int elementsPerPage = partSelectors.Length;
        int startIndex = (currentPage - 1) * elementsPerPage;
        int endIndex = Mathf.Min(startIndex + elementsPerPage, parts.Count);

        int selectorIndex = 0;

        for (int i = startIndex; i < endIndex; i++)
        {
            var entry = parts[i];

            NavItem partSelector = partSelectors[selectorIndex];
            partSelector.onSelected.RemoveAllListeners();

            GameObject obj = Instantiate(Assets.i.editorShipPartPrefab, itemPanel);
            
            EditorShipPart part = obj.GetComponent<EditorShipPart>();
            part.Init(entry.data);

            part.rtf.enabled = true;
            part.rtf.target = partSelector.rect;

            shownParts.Add(part);

            partCounters[selectorIndex].SetCount(entry.count);

            partSelector.onSelected.AddListener(() =>
            {
                nav.OnInventoryPartGrabbed(entry.data);
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
        var parts = partInventory.GetParts(showState);
        ShowParts(parts);
    }

    public EditorShipPart CreateInventoryPart(ShipPartData data)
    {
        var obj = Instantiate(Assets.i.editorShipPartPrefab);
        var part = obj.GetComponent<EditorShipPart>();
        part.Init(data);
        return part;
    }

    public bool TryTakePart(ShipPartData data, out EditorShipPart part)
    {
        part = null;

        if (!partInventory.TryTake(data))
            return false;

        part = CreateInventoryPart(data);
        RefreshCurrentPage();
        return true;
    }

    public void AddPart(ShipPartData data)
    {
        partInventory.Add(data);
        RefreshCurrentPage();
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
