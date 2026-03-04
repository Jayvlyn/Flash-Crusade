using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollMenuManager : MonoBehaviour
{
    [SerializeField] StreamlinedScrollHelper scrollHelper;
    [SerializeField] private RectTransform grid;
    [SerializeField] private NavItem[] primaryNavItems;
    [SerializeField] private NavItem[] bufferNavItems;

    List<RectTransform> shownRects;
    List<RectTransform> nextRects;

    [SerializeField] private Pager pager;

    public void ShowPageOne(IReadOnlyList<RectTransform> collection)
    {
        SetPage(collection);
        scrollHelper.OnPageChange();
    }

    public void ScrollDown(IReadOnlyList<RectTransform> collection)
    {
        pager.PageDown();
        SetPage(collection, true, true);
        DoSmoothScroll(true);
        scrollHelper.OnPageChange();
    }

    public void ScrollUp(IReadOnlyList<RectTransform> collection)
    {
        pager.PageUp();
        DoSmoothScroll(false);
        SetPage(collection, true, false);
        scrollHelper.OnPageChange();
    }

    // expects a list of items to display that already have data initialized on them
    void SetPage(IReadOnlyList<RectTransform> collection, bool scrolling = false, bool scrollingDown = false)
    {
        if(scrolling) nextRects = new();
        else shownRects = new();

        int elementsPerPage = primaryNavItems.Length;

        pager.Recalculate(collection.Count, elementsPerPage);
        var (startIndex, endIndex) = pager.GetRange(collection.Count, elementsPerPage);

        int navItemIndex = 0;
        for (int i = startIndex; i < endIndex; i++, navItemIndex++)
        {
            var entry = collection[i];

            NavItem primary = primaryNavItems[navItemIndex];
            NavItem buffer = bufferNavItems[navItemIndex];
            if(scrollingDown)
                entry.transform.SetParent(buffer.transform);
            else 
                entry.transform.SetParent(primary.transform);
                
            
            entry.gameObject.SetActive(true);
            entry.transform.localScale = Vector3.one;
            entry.transform.SetAsFirstSibling();

            if(scrolling)
            {
                nextRects.Add(entry);
            }
            else 
                shownRects.Add(entry);
        }
    }

    void DoSmoothScroll(bool scrollDown = true)
    {
        if (scrollRoutine != null) StopCoroutine(scrollRoutine);
        scrollRoutine = StartCoroutine(SmoothScroll(scrollDown, 0.2f));
    }

    Coroutine scrollRoutine;

    IEnumerator SmoothScroll(bool scrollDown = true, float duration = 0.25f)
    {
        NavState.Scrolling = true;
        float elapsed = 0f;

        float startY, targetY;
        if (scrollDown)
        {
            startY = 0;
            targetY = grid.sizeDelta.y;
        }
        else
        {
            startY = grid.sizeDelta.y;
            targetY = 0;
            Wrap(scrollDown);
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
            Wrap(scrollDown);
            grid.anchoredPosition = new Vector2(grid.anchoredPosition.x, startY);
        }

        SetShownRects();

        NavState.Scrolling = false;
    }

    void SetShownRects()
    {
        ClearShownRects();

        for (int i = 0; i < nextRects.Count; i++)
            shownRects.Add(nextRects[i]);

        nextRects.Clear();
    }

    // case1: wrap back to primary from buffer (wrap, start scroll)
    // case2: wrap primary to buffer (finish scroll, then wrap)
    void Wrap(bool scrollDown = true)
    {
        int elementsPerPage = primaryNavItems.Length;

        NavItem[] sourceItems;
        NavItem[] targetItems;

        if (scrollDown)
        {
            sourceItems = bufferNavItems;
            targetItems = primaryNavItems;
        }
        else
        {
            sourceItems = primaryNavItems;
            targetItems = bufferNavItems;
        }

        for (int i = 0; i < elementsPerPage; i++)
        {
            NavItem sourceItem = sourceItems[i];
            NavItem targetItem = targetItems[i];

            if (sourceItem.transform.childCount > 0)
            {
                Transform item = sourceItem.transform.GetChild(0);
                item.SetParent(targetItem.transform, false);
                item.SetAsFirstSibling();
            }
        }
    }

    void ClearShownRects()
    {
        if (shownRects == null) shownRects = new();
        foreach (var rect in shownRects)
            rect.gameObject.SetActive(false);
        shownRects.Clear();
    }
}
