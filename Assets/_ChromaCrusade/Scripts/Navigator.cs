using UnityEngine;

public abstract class Navigator : MonoBehaviour, IInitializable
{
    [SerializeField] protected NavVisualizer visualizer;

    protected virtual void Start()
    {
        if (visualizer == null) visualizer = FindFirstObjectByType<NavVisualizer>();
        visualizer.gameObject.SetActive(true);
    }

    public abstract void Init();

    public abstract void TriggerNav(Vector2 dir);
}
