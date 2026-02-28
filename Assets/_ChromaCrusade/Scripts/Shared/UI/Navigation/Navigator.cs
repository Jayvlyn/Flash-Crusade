using UnityEngine;

public abstract class Navigator : MonoBehaviour, IInitializable
{
    protected NavVisualizer visualizer;

    public virtual void Init()
    {
        if (visualizer == null) visualizer = FindFirstObjectByType<NavVisualizer>();
        visualizer.gameObject.SetActive(true);
    }

    public void VisualHighlight(NavItem item)
    {
        visualizer.HighlightItem(item);
    }
}
