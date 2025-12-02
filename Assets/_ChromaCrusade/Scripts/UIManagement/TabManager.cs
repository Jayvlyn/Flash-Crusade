using System.Collections;
using UnityEngine;

public class TabManager : MonoBehaviour
{
    public enum Orientation { Vertical, Horizontal }
    public Orientation orientation = Orientation.Vertical;

    [Header("Tab Sizing")]
    [Tooltip("Inactive width (vertical) or height (horizontal) in normalized units")]
    [Range(0f, 1f)] public float inactiveSize = 0.1f;

    [Tooltip("Active width (vertical) or height (horizontal) in normalized units")]
    [Range(0f, 1f)] public float activeSize = 0.2f;

    [Tooltip("Margin between tabs along the layout axis (normalized 0-1)")]
    [Range(0f, 0.1f)] public float tabMargin = 0.01f;

    public NavTab[] tabs;
    public NavVisualizer navVisualizer;

    private NavTab activeTab;
    private NavTab lastActiveTab;
    private Coroutine tabRoutine;

    private void Awake()
    {
        foreach (var t in tabs)
            t.owner = this;

        UpdateTabAnchors();
    }

    public void SwitchToTab(NavTab newTab)
    {
        if (activeTab == newTab) return;

        lastActiveTab = activeTab;
        activeTab = newTab;

        if (lastActiveTab) lastActiveTab.selected = false;

        if (UIManager.Smoothing)
        {
            if (tabRoutine != null) StopCoroutine(tabRoutine);
            tabRoutine = StartCoroutine(LerpTabs());
        }
        else
        {
            SnapTabs();
        }
    }

    private void UpdateTabAnchors()
    {
        int count = tabs.Length;
        if (count == 0) return;

        float axisMarginTotal = tabMargin * (count + 1);
        float axisSize = (1f - axisMarginTotal) / count;

        for (int i = 0; i < count; i++)
        {
            var tab = tabs[i];
            float totalTabHeight = axisSize * count + tabMargin * (count + 1);
            float startPos = 1f - tabMargin - (i + 1) * axisSize - i * tabMargin;

            if (orientation == Orientation.Vertical)
            {
                tab.rect.anchorMin = new Vector2(0f, startPos);
                tab.rect.anchorMax = new Vector2(inactiveSize, startPos + axisSize);

                tab.anchorMinInactive = new Vector2(0f, startPos);
                tab.anchorMaxInactive = new Vector2(inactiveSize, startPos + axisSize);

                tab.anchorMinActive = new Vector2(0f, startPos);
                tab.anchorMaxActive = new Vector2(activeSize, startPos + axisSize);
            }
            else
            {
                tab.rect.anchorMin = new Vector2(startPos, 0f);
                tab.rect.anchorMax = new Vector2(startPos + axisSize, inactiveSize);

                tab.anchorMinInactive = new Vector2(startPos, 0f);
                tab.anchorMaxInactive = new Vector2(startPos + axisSize, inactiveSize);

                tab.anchorMinActive = new Vector2(startPos, 0f);
                tab.anchorMaxActive = new Vector2(startPos + axisSize, activeSize);
            }

            tab.rect.anchorMin = tab.anchorMinInactive;
            tab.rect.anchorMax = tab.anchorMaxInactive;

            tab.rect.anchoredPosition = Vector2.zero;
            tab.rect.sizeDelta = Vector2.zero;
        }
    }

    private IEnumerator LerpTabs()
    {
        float duration = 0.15f;
        float t = 0f;

        Vector2 startMin = activeTab.rect.anchorMin;
        Vector2 startMax = activeTab.rect.anchorMax;
        Vector2 lastMin = lastActiveTab ? lastActiveTab.rect.anchorMin : Vector2.zero;
        Vector2 lastMax = lastActiveTab ? lastActiveTab.rect.anchorMax : Vector2.zero;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float s = Mathf.SmoothStep(0, 1, t);

            activeTab.rect.anchorMin = Vector2.Lerp(startMin, activeTab.anchorMinActive, s);
            activeTab.rect.anchorMax = Vector2.Lerp(startMax, activeTab.anchorMaxActive, s);
            activeTab.rect.anchoredPosition = Vector2.zero;
            activeTab.rect.sizeDelta = Vector2.zero;

            if (lastActiveTab)
            {
                lastActiveTab.rect.anchorMin = Vector2.Lerp(lastMin, lastActiveTab.anchorMinInactive, s);
                lastActiveTab.rect.anchorMax = Vector2.Lerp(lastMax, lastActiveTab.anchorMaxInactive, s);
                lastActiveTab.rect.anchoredPosition = Vector2.zero;
                lastActiveTab.rect.sizeDelta = Vector2.zero;
            }

            navVisualizer.UpdateCurrentItemImmediate(activeTab);
            yield return null;
        }

        SnapTabs();
    }

    private void SnapTabs()
    {
        if (activeTab != null)
        {
            activeTab.rect.anchorMin = activeTab.anchorMinActive;
            activeTab.rect.anchorMax = activeTab.anchorMaxActive;
            activeTab.rect.anchoredPosition = Vector2.zero;
            activeTab.rect.sizeDelta = Vector2.zero;
        }

        if (lastActiveTab != null)
        {
            lastActiveTab.rect.anchorMin = lastActiveTab.anchorMinInactive;
            lastActiveTab.rect.anchorMax = lastActiveTab.anchorMaxInactive;
            lastActiveTab.rect.anchoredPosition = Vector2.zero;
            lastActiveTab.rect.sizeDelta = Vector2.zero;
        }

        Canvas.ForceUpdateCanvases();
        if (activeTab != null) navVisualizer.UpdateCurrentItemImmediate(activeTab);
    }
}
