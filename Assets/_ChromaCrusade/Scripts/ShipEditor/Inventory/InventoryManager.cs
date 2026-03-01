using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour, IInventoryManager
{
    [Header("Refs")]
    public RectTransform grid;
    public RectTransform defaultPartSpawn;
    public NavItem[] partSelectors;

    PartCounter[] partCounters;
    List<ShipPart> shownParts;
    List<ShipPart> nextParts;
    PartInventoryModel partInventory;
    [SerializeField] private Pager pager;

    public Pager GetPager() => pager;

    public bool InCreativeMode => EditorState.context == EditorContext.Creative;

    #region Initialization 

    private void OnEnable()
    {
        EventBus.Subscribe<ScrollInputEvent>(OnScrollInput);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ScrollInputEvent>(OnScrollInput);
    }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        shownParts = new List<ShipPart>();

        if(InCreativeMode)
        {
            ShipPartList list = InventoryLoader.LoadFullList();
            partInventory = new PartInventoryModel(list);
        }
        else
        {
            PartInventory inv = InventoryLoader.Load();
            partInventory = new PartInventoryModel(inv);
        }

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

        if(!InCreativeMode)
        {
            if (!partInventory.TryTake(data))
                return false;
        }

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
        SetPage(parts, elementsPerPage, nextParts);
        EventBus.Publish(new InventoryPageChangedEvent());
    }

    void ScrollUp()
    {
        var parts = partInventory.GetParts(showState);
        nextParts = new List<ShipPart>();
        DoSmoothScroll(false);

        SetPage(parts, 0, nextParts);
        EventBus.Publish(new InventoryPageChangedEvent());
    }

    void ShowParts()
    {
        var parts = partInventory.GetParts(showState);
        ClearParts();
        SetPage(parts, 0, shownParts);
        EventBus.Publish(new InventoryPageChangedEvent());
    }

    void SetPage(IReadOnlyList<PartInventoryModel.Entry> parts, int selectorStartIndex, List<ShipPart> targetList)
    {
        int elementsPerPage = partSelectors.Length / 2;

        pager.Recalculate(parts.Count, elementsPerPage);
        var (startIndex, endIndex) = pager.GetRange(parts.Count, elementsPerPage);

        int primarySelectorIndex = 0;
        int selectorIndex = selectorStartIndex;

        for (int i = 0; i < partSelectors.Length/2; i++)
        {
            partSelectors[i].onSelected.RemoveAllListeners();
        }

        for (int i = startIndex; i < endIndex; i++)
        {
            var entry = parts[i];

            NavItem selector = partSelectors[selectorIndex];
            NavItem primarySelector = partSelectors[primarySelectorIndex];

            GameObject obj = Instantiate(Assets.i.editorShipPartPrefab, selector.transform);
            obj.transform.SetAsFirstSibling();

            ShipPart part = obj.GetComponent<ShipPart>();
            part.Init(entry.data);

            targetList.Add(part);

            if(!InCreativeMode)
                partCounters[selectorIndex].SetCount(entry.count);
            else
                partCounters[selectorIndex].SetCount(0);

            var capturedData = entry.data;
            primarySelector.onSelected.AddListener(() =>
            {
                EventBus.Publish(new InventoryPartGrabbedEvent { part = capturedData });
            });

            primarySelectorIndex++;
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
        scrollRoutine = StartCoroutine(SmoothScroll(scrollDown, 0.2f));
    }

    Coroutine scrollRoutine;

    IEnumerator SmoothScroll(bool scrollDown = true, float duration = 0.5f)
    {
        EditorState.Scrolling = true;
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

        EditorState.Scrolling = false;
    }


    /// <summary>
    /// At all times the resting state should show the active parts on the primary selectors, with the secondary selectors resting below them.
    /// 
    /// Two different use cases for this wrap. 
    /// Case 1: Need to scroll down -> set new parts to secondary selectors -> scroll down -> *snap new parts back to the primary selectors*
    /// Case 2: Need to scroll up -> *snap current parts to secondary selectors&* -> set new parts to primary selectors -> scroll up
    /// </summary>
    /// <param name="case1"></param>
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
        var prevState = this.showState;
        this.showState = showState;
        pager.Reset();

        switch (prevState)
        {
            case PartType.Cabin:
                ScrollDown();
                break;
            case PartType.Core:
                if (showState == PartType.Cabin) ScrollUp();
                else ScrollDown();
                break;
            case PartType.Wing:
                if (showState == PartType.Cabin || showState == PartType.Core) ScrollUp();
                else ScrollDown();
                break;
            case PartType.Weapon:
                if (showState == PartType.Utility) ScrollDown();
                else ScrollUp();
                break;
            case PartType.Utility:
                ScrollUp();
                break;
            default:
                ShowParts();
                break;
        }
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

    public void OnScrollInput(ScrollInputEvent e)
    {
        if (e.scrollDirection == ScrollDirection.Up) OnUpSelected();
        else                                         OnDownSelected();
    }

    public void OnUpSelected()
    {
        if (EditorState.Scrolling) return;
        if (pager.PageUp()) ScrollUp();
    }

    public void OnDownSelected()
    {
        if (EditorState.Scrolling) return;
        if (pager.PageDown()) ScrollDown();
    }

    #endregion
}
