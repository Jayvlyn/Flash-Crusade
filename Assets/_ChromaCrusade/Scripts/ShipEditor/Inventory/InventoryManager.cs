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
        ShowParts();
        return true;
    }

    public void AddPart(ShipPartData data)
    {
        partInventory.Add(data);
        ShowParts();
    }

    #endregion

    #region View Rendering

    void ScrollDown()
    {
        var parts = partInventory.GetParts(showState);
        nextParts = new List<ShipPart>();
        DoSmoothScroll(true);

        int elementsPerPage = partSelectors.Length / 2;
        RenderPage(parts, elementsPerPage, nextParts);
        EventBus.Publish(new InventoryPageChangedEvent());
    }

    void ScrollUp()
    {
        var parts = partInventory.GetParts(showState);
        nextParts = new List<ShipPart>();
        DoSmoothScroll(false);

        RenderPage(parts, 0, nextParts);
        EventBus.Publish(new InventoryPageChangedEvent());
    }

    void ShowParts()
    {
        var parts = partInventory.GetParts(showState);
        ClearParts();
        RenderPage(parts, 0, shownParts);
        EventBus.Publish(new InventoryPageChangedEvent());
    }

    void RenderPage(IReadOnlyList<PartInventoryModel.Entry> parts, int selectorStartIndex, List<ShipPart> targetList)
    {
        int elementsPerPage = partSelectors.Length / 2;

        pager.Recalculate(parts.Count, elementsPerPage);
        var (startIndex, endIndex) = pager.GetRange(parts.Count, elementsPerPage);

        int selectorIndex = selectorStartIndex;

        for (int i = startIndex; i < endIndex; i++)
        {
            var entry = parts[i];

            NavItem selector = partSelectors[selectorIndex];
            selector.onSelected.RemoveAllListeners();

            GameObject obj = Instantiate(Assets.i.editorShipPartPrefab, selector.transform);
            obj.transform.SetAsFirstSibling();

            ShipPart part = obj.GetComponent<ShipPart>();
            part.Init(entry.data);

            targetList.Add(part);
            partCounters[selectorIndex].SetCount(entry.count);

            var capturedData = entry.data;
            selector.onSelected.AddListener(() =>
            {
                EventBus.Publish(new InventoryPartGrabbedEvent { part = capturedData });
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

    void DoSmoothScroll(bool scrollDown = true)
    {
        if(scrollRoutine != null) StopCoroutine(scrollRoutine);
        scrollRoutine = StartCoroutine(SmoothScroll(scrollDown, 0.5f));
    }

    Coroutine scrollRoutine;

    IEnumerator SmoothScroll(bool scrollDown = true, float duration = 0.5f)
    {
        Scrolling = true;
        float elapsed = 0f;

        float startY, targetY;
        if(scrollDown)
        {
            startY = 0;
            targetY = grid.sizeDelta.y - 4f;
        }
        else
        {
            startY = grid.sizeDelta.y - 4f;
            targetY = 0;
            WrapSelectors(scrollDown);
            grid.anchoredPosition = new Vector2(grid.anchoredPosition.x, startY);
        }

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

        if (scrollDown)
        {
            WrapSelectors(scrollDown);
            grid.anchoredPosition = new Vector2(grid.anchoredPosition.x, startY);
        }

        ClearParts();

        for (int i = 0; i < nextParts.Count; i++)
        {
            shownParts.Add(nextParts[i]);
        }

        Scrolling = false;
    }


    /// <summary>
    /// At all times the resting state should show the active parts on the primary selectors, with the secondary selectors resting below them.
    /// 
    /// Two different use cases for this wrap. 
    /// Case 1: Need to scroll down -> set new parts to secondary selectors -> scroll down -> *snap new parts back to the primary selectors*
    /// Case 2: Need to scroll up -> *snap current parts to secondary selectors&* -> set new parts to primary selectors -> scroll up
    /// </summary>
    /// <param name="wrapBackUp"></param>
    void WrapSelectors(bool case1 = true)
    {
        int elementsPerPage = partSelectors.Length / 2;
        int selectorIndex, startIndex, endIndex;

        if(case1)
        {
            selectorIndex = 0;

            startIndex = elementsPerPage;
            endIndex = partSelectors.Length;
        }
        else
        {
            selectorIndex = elementsPerPage;

            startIndex = 0;
            endIndex = elementsPerPage;
        }

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
    }

    #endregion

    #region State

    PartType showState;

    void ChangeShowState(PartType showState)
    {
        this.showState = showState;
        pager.Reset();
        ShowParts();
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
        if (Scrolling) return;
        if (pager.PageUp()) ScrollUp();
    }

    public void OnDownSelected()
    {
        if (Scrolling) return;
        if (pager.PageDown()) ScrollDown();
    }

    #endregion
}
