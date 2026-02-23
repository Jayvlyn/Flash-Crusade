using UnityEngine;

public abstract class Navigator : MonoBehaviour, IInitializable
{
    protected NavVisualizer visualizer;
    public NavState NavState { get; set; }

    public virtual void Init()
    {
        if (visualizer == null) visualizer = FindFirstObjectByType<NavVisualizer>();
        visualizer.gameObject.SetActive(true);

        if(NavState == null)
        {
            NavState = new NavState();
            visualizer.NavState = NavState;
        }
    }
}
