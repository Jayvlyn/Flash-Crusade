using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollMenuManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] StreamlinedScrollHelper scrollHelper;
    [SerializeField] RectTransform grid;
    [SerializeField] NavItem[] primaryNavItems;
    [SerializeField] NavItem[] bufferNavItems;
    [SerializeField] Pager pager;

    [Header("Config")]
    [SerializeField] float scrollYTargetOffset = 0f;

    List<RectTransform> shownRects = new();
    List<RectTransform> nextRects = new();

    Coroutine scrollRoutine;

    int ElementsPerPage => primaryNavItems.Length;
    float PageHeight => grid.sizeDelta.y + scrollYTargetOffset;

    public void ShowPageOne(IReadOnlyList<RectTransform> collection)
    {
        SetPage(collection, false, false);
        scrollHelper.OnPageChange();
    }

    public void ScrollDown(IReadOnlyList<RectTransform> collection)
    {
        pager.PageDown();
        SetPage(collection, true, true);
        StartSmoothScroll(true);
        scrollHelper.OnPageChange();
    }

    public void ScrollUp(IReadOnlyList<RectTransform> collection)
    {
        pager.PageUp();
        StartSmoothScroll(false);
        SetPage(collection, true, false);
        scrollHelper.OnPageChange();
    }

    void SetPage(IReadOnlyList<RectTransform> collection, bool scrolling, bool scrollingDown)
    {
        var targetList = scrolling ? nextRects : shownRects;
        if (scrolling) targetList.Clear();
        else ClearShownRects();

        pager.Recalculate(collection.Count, ElementsPerPage);
        var (start, end) = pager.GetRange(collection.Count, ElementsPerPage);

        for (int i = start, slot = 0; i < end; i++, slot++)
        {
            var entry = collection[i];
            var parent = scrollingDown ? bufferNavItems[slot] : primaryNavItems[slot];

            entry.SetParent(parent.transform, false);
            entry.gameObject.SetActive(true);
            entry.localScale = Vector3.one;
            entry.SetAsFirstSibling();

            targetList.Add(entry);
        }
    }

    void StartSmoothScroll(bool scrollDown)
    {
        if (scrollRoutine != null)
            StopCoroutine(scrollRoutine);

        scrollRoutine = StartCoroutine(SmoothScroll(scrollDown, 0.2f));
    }

    IEnumerator SmoothScroll(bool scrollDown, float duration)
    {
        NavState.Scrolling = true;

        float startY = scrollDown ? 0f : PageHeight;
        float targetY = scrollDown ? PageHeight : 0f;

        if (!scrollDown)
        {
            Wrap(false);
            SetGridY(startY);
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            SetGridY(Mathf.LerpUnclamped(startY, targetY, eased));
            yield return null;
        }

        SetGridY(targetY);

        if (scrollDown)
        {
            Wrap(true);
            SetGridY(startY);
        }

        CommitNextAsShown();
        NavState.Scrolling = false;
    }

    void CommitNextAsShown()
    {
        ClearShownRects();
        shownRects.AddRange(nextRects);
        nextRects.Clear();
    }

    void Wrap(bool scrollDown)
    {
        var source = scrollDown ? bufferNavItems : primaryNavItems;
        var target = scrollDown ? primaryNavItems : bufferNavItems;

        for (int i = 0; i < ElementsPerPage; i++)
        {
            if (source[i].transform.childCount == 0)
                continue;

            var child = source[i].transform.GetChild(0);
            child.SetParent(target[i].transform, false);
            child.SetAsFirstSibling();
        }
    }

    void ClearShownRects()
    {
        foreach (var rect in shownRects)
            rect.gameObject.SetActive(false);

        shownRects.Clear();
    }

    void SetGridY(float y)
    {
        grid.anchoredPosition = new Vector2(grid.anchoredPosition.x, y);
    }
}