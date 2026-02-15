using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour, IInventoryManager
{
    public static bool Scrolling = false;

    [Header("Refs")]
    public RectTransform grid;
    public RectTransform defaultPartSpawn;
    public NavItem[] partSelectors;

    private PartCounter[] partCounters;
    private List<ShipPart> shownParts;
    private List<ShipPart> nextParts;
    private PartInventoryModel partInventory;
    private PartInventoryPager pager;

    public PartInventoryPager GetPager() => pager;

    #region Initialization 

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        shownParts = new List<ShipPart>();

        PartInventory inv = InventoryLoader.Load();
        partInventory = new PartInventoryModel(inv);

        pager = new PartInventoryPager();

        partCounters = new PartCounter[partSelectors.Length];

        for (int i = 0; i < partSelectors.Length; i++)
            partCounters[i] = partSelectors[i].rect.GetChild(0).GetComponent<PartCounter>();   
    }

    #endregion

    #region IInventoryManager

    public void SetPartToDefaultStart(ShipPart part)
    {
        part.rect.SetParent(defaultPartSpawn.parent, worldPositionStays: false);
        part.rtf.target = defaultPartSpawn;
        part.rtf.stretch = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(defaultPartSpawn);
        part.rtf.enabled = true;
        part.rtf.Follow();
    }

    public bool TryTakePart(ShipPartData data, out ShipPart part)
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

    void RefreshCurrentPage()
    {
        var parts = partInventory.GetParts(showState);
        ShowParts(parts);
        EventBus.Publish(new InventoryPageChangedEvent());
    }

    void SlidePage()
    {
        var parts = partInventory.GetParts(showState);
        ShowPartsNextPage(parts);
        EventBus.Publish(new InventoryPageChangedEvent());
    }

    void ShowPartsNextPage(IReadOnlyList<PartInventoryModel.Entry> parts)
    {
        DoSmoothScroll();

        int elementsPerPage = partSelectors.Length / 2;
        pager.Recalculate(parts.Count, elementsPerPage);
        var (startIndex, endIndex) = pager.GetRange(parts.Count, elementsPerPage);

        nextParts = new List<ShipPart>();

        int selectorIndex = elementsPerPage;
        for (int i = startIndex; i < endIndex; i++)
        {
            var entry = parts[i];

            NavItem partSelector = partSelectors[selectorIndex];
            partSelector.onSelected.RemoveAllListeners();

            GameObject obj = Instantiate(Assets.i.editorShipPartPrefab, partSelector.transform);
            obj.transform.SetAsFirstSibling();

            ShipPart part = obj.GetComponent<ShipPart>();
            part.Init(entry.data);

            nextParts.Add(part);

            partCounters[selectorIndex].SetCount(entry.count);

            partSelector.onSelected.AddListener(() =>
            {
                EventBus.Publish(new InventoryPartGrabbedEvent { part = entry.data });
            });

            selectorIndex++;
        }

        for (int i = selectorIndex; i < partCounters.Length; i++)
            partCounters[i].SetCount(0);
    }

    void ShowParts(IReadOnlyList<PartInventoryModel.Entry> parts)
    {
        ClearParts();

        int elementsPerPage = partSelectors.Length / 2;
        pager.Recalculate(parts.Count, elementsPerPage);
        var (startIndex, endIndex) = pager.GetRange(parts.Count, elementsPerPage);


        int selectorIndex = 0;
        for (int i = startIndex; i < endIndex; i++)
        {
            var entry = parts[i];

            NavItem partSelector = partSelectors[selectorIndex];
            partSelector.onSelected.RemoveAllListeners();

            GameObject obj = Instantiate(Assets.i.editorShipPartPrefab, partSelector.transform);
            obj.transform.SetAsFirstSibling();

            ShipPart part = obj.GetComponent<ShipPart>();
            part.Init(entry.data);

            shownParts.Add(part);

            partCounters[selectorIndex].SetCount(entry.count);

            partSelector.onSelected.AddListener(() =>
            {
                EventBus.Publish(new InventoryPartGrabbedEvent { part = entry.data });
            });

            selectorIndex++;
        }

        for (int i = selectorIndex; i < partCounters.Length; i++)
            partCounters[i].SetCount(0);
    }

    void ClearParts()
    {
        foreach (var part in shownParts)
            Destroy(part.gameObject);
        shownParts.Clear();
    }

    void DoSmoothScroll()
    {
        if(scrollRoutine != null) StopCoroutine(scrollRoutine);
        scrollRoutine = StartCoroutine(SmoothScroll(0.5f));
    }

    Coroutine scrollRoutine;

    IEnumerator SmoothScroll(float duration)
    {
        Scrolling = true;
        float elapsed = 0f;

        float startY = grid.anchoredPosition.y;
        float targetY = grid.sizeDelta.y - 4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            float y = Mathf.LerpUnclamped(startY, targetY, eased);
            grid.anchoredPosition = new Vector2(grid.anchoredPosition.x, y);

            yield return null;
        }

        grid.anchoredPosition = new Vector2(grid.anchoredPosition.x, targetY);

        WrapSelectors();

        grid.anchoredPosition = new Vector2(grid.anchoredPosition.x, 0);
        Scrolling = false;
    }

    void WrapSelectors()
    {
        int selectorIndex = 0;
        int elementsPerPage = partSelectors.Length / 2;
        int startIndex = 18;
        int endIndex = 36;
        for (int i = startIndex; i < endIndex; i++)
        {
            NavItem sourceSelector = partSelectors[i];
            NavItem targetSelector = partSelectors[selectorIndex];

            if (sourceSelector.transform.childCount > 1)
            {
                Transform part = sourceSelector.transform.GetChild(0);
                part.SetParent(targetSelector.transform, false);
                part.SetAsFirstSibling();
            }

            PartCounter originCounter = partCounters[i];
            PartCounter targetCounter = partCounters[selectorIndex];

            Debug.Log(targetSelector.transform.childCount);

            if (targetSelector.transform.childCount < 2)
            {
                targetCounter.countText.text = "";
            }
            else
            {
                targetCounter.countText.text = originCounter.countText.text;
            }

            selectorIndex++;
        }

        ClearParts();

        for (int i = 0; i < nextParts.Count; i++)
        {
            shownParts.Add(nextParts[i]);
        }
    }

    #endregion

    #region State

    PartType showState;

    void ChangeShowState(PartType showState)
    {
        this.showState = showState;
        pager.Reset();
        RefreshCurrentPage();
    }

    #endregion

    #region Factory

    ShipPart CreateInventoryPart(ShipPartData data)
    {
        var obj = Instantiate(Assets.i.editorShipPartPrefab);
        var part = obj.GetComponent<ShipPart>();
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
        if(pager.PageUp()) SlidePage();
    }

    public void OnDownSelected()
    {
        if(pager.PageDown()) SlidePage();
    }

    #endregion
}
