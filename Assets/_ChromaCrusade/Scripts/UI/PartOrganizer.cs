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
    private PartInventoryModel partInventory;
    private PartInventoryPager pager;

    #region Initialization 

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        shownParts = new List<EditorShipPart>();

        PartInventory inv = InventoryLoader.Load();
        partInventory = new PartInventoryModel(inv);

        pager = new PartInventoryPager();

        partCounters = new PartCounter[partSelectors.Length];

        for (int i = 0; i < partSelectors.Length; i++)
            partCounters[i] = partSelectors[i].rect.GetChild(0).GetComponent<PartCounter>();   
    }

    #endregion

    #region Public API

    public void SetPartToDefaultStart(EditorShipPart part)
    {
        part.rect.SetParent(defaultPartSpawn.parent, worldPositionStays: false);
        part.rtf.target = defaultPartSpawn;
        part.rtf.stretch = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(defaultPartSpawn);
        part.rtf.enabled = true;
        part.rtf.Follow();
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

    #endregion

    #region View Rendering

    private void RefreshCurrentPage()
    {
        var parts = partInventory.GetParts(showState);
        ShowParts(parts);
    }

    private void ShowParts(IReadOnlyList<PartInventoryModel.Entry> parts)
    {
        ClearParts();

        int elementsPerPage = partSelectors.Length;
        pager.Recalculate(parts.Count, elementsPerPage);
        var (startIndex, endIndex) = pager.GetRange(parts.Count, elementsPerPage);


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

    private void ClearParts()
    {
        foreach (var part in shownParts)
            Destroy(part.gameObject);
        shownParts.Clear();
    }

    #endregion

    #region State

    private PartType showState;

    private void ChangeShowState(PartType showState)
    {
        this.showState = showState;
        pager.Reset();
        RefreshCurrentPage();
    }

    #endregion

    #region Factory

    private EditorShipPart CreateInventoryPart(ShipPartData data)
    {
        var obj = Instantiate(Assets.i.editorShipPartPrefab);
        var part = obj.GetComponent<EditorShipPart>();
        part.Init(data);
        return part;
    }

    #endregion

    #region Event Responses

    public void OnCabinTabSelected() => ChangeShowState(PartType.Cabin);
    public void OnCoreTabSelected() => ChangeShowState(PartType.Core);
    public void OnWingTabSelected() => ChangeShowState(PartType.Wing);
    public void OnWeaponTabSelected() => ChangeShowState(PartType.Weapon);
    public void OnUtilityTabSelected() => ChangeShowState(PartType.Utility);

    public void OnUpSelected()
    {
        if(pager.PageUp()) RefreshCurrentPage();
    }

    public void OnDownSelected()
    {
        if(pager.PageDown()) RefreshCurrentPage();
    }

    #endregion
}
