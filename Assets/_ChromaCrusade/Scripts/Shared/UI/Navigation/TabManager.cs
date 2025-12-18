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

    private NavTab activeTab;
    private NavTab lastActiveTab;
    private Coroutine tabRoutine;

    private void Awake()
    {
        foreach (var t in tabs)
            t.owner = this;

        UpdateTabAnchors();
    }

    private void Start()
    {
        SwitchToTab(tabs[0]);
        tabs[0].OnSelected();
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
            t += Time.deltaTime / duration;
            float s = Mathf.SmoothStep(0, 1, t);

            LerpAnchors(activeTab.rect, startMin, startMax, activeTab.anchorMinActive, activeTab.anchorMaxActive, s);

            if (lastActiveTab != null)
                LerpAnchors(lastActiveTab.rect, lastMin, lastMax, lastActiveTab.anchorMinInactive, lastActiveTab.anchorMaxInactive, s);

            EventBus.Publish(new TabSizeUpdatedEvent());
            yield return null;
        }

        SnapTabs();
    }

    private void SnapTabs()
    {
        if (activeTab != null) SetTabState(activeTab, true);
        if (lastActiveTab != null) SetTabState(lastActiveTab, false);

        Canvas.ForceUpdateCanvases();
        if (activeTab != null) EventBus.Publish(new TabSizeUpdatedEvent());
    }

    #region Anchor Helpers

    private void ApplyAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    private void SetTabState(NavTab tab, bool active)
    {
        if (active)
            ApplyAnchors(tab.rect, tab.anchorMinActive, tab.anchorMaxActive);
        else
            ApplyAnchors(tab.rect, tab.anchorMinInactive, tab.anchorMaxInactive);
    }

    private void LerpAnchors(RectTransform rect, Vector2 startMin, Vector2 startMax, Vector2 targetMin, Vector2 targetMax, float t)
    {
        Vector2 min = Vector2.Lerp(startMin, targetMin, t);
        Vector2 max = Vector2.Lerp(startMax, targetMax, t);
        ApplyAnchors(rect, min, max);
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

            if (orientation == Orientation.Vertical)
            {
                float startPos = 1f - tabMargin - (i + 1) * axisSize - i * tabMargin;

                tab.anchorMinInactive = new Vector2(0f, startPos);
                tab.anchorMaxInactive = new Vector2(inactiveSize, startPos + axisSize);

                tab.anchorMinActive = new Vector2(0f, startPos);
                tab.anchorMaxActive = new Vector2(activeSize, startPos + axisSize);
            }
            else
            {
                float startPos = tabMargin + i * (axisSize + tabMargin);

                tab.anchorMinInactive = new Vector2(startPos, 0f);
                tab.anchorMaxInactive = new Vector2(startPos + axisSize, inactiveSize);

                tab.anchorMinActive = new Vector2(startPos, 0f);
                tab.anchorMaxActive = new Vector2(startPos + axisSize, activeSize);
            }

            ApplyAnchors(tab.rect, tab.anchorMinInactive, tab.anchorMaxInactive);
        }
    }

    #endregion
}
