using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    public NavTab[] tabs;
    public Vector2 inactiveTabSize, activeTabSize;
    public NavVisualizer navVisualizer;

    private NavTab activeTab;
    private NavTab lastActiveTab;

    private void Awake()
    {
        foreach (var t in tabs)
            t.owner = this;
    }

    public void SwitchToTab(NavTab newTab)
    {
        lastActiveTab = activeTab;
        activeTab = newTab;

        if(lastActiveTab) lastActiveTab.selected = false;

        if(tabRoutine != null)
            StopCoroutine(tabRoutine);

        tabRoutine = StartCoroutine(LerpTabs());
    }

    private Coroutine tabRoutine;
    private IEnumerator LerpTabs()
    {
        float duration = 0.15f;
        float t = 0f;

        Vector2 startActiveSize = activeTab.rect.sizeDelta;
        Vector2 startInactiveSize = lastActiveTab ? lastActiveTab.rect.sizeDelta : Vector2.zero;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float s = Mathf.SmoothStep(0, 1, t);

            activeTab.rect.sizeDelta = Vector2.Lerp(startActiveSize, activeTabSize, s);
            navVisualizer.UpdateCurrentItemImmediate(activeTab);

            if (lastActiveTab)
                lastActiveTab.rect.sizeDelta = Vector2.Lerp(startInactiveSize, inactiveTabSize, s);


            yield return null;
        }

        activeTab.rect.sizeDelta = activeTabSize;
        navVisualizer.UpdateCurrentItemImmediate(activeTab);

        if (lastActiveTab)
            lastActiveTab.rect.sizeDelta = inactiveTabSize;
    }
}
