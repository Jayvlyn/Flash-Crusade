using UnityEngine;

public abstract class Navigator : MonoBehaviour, IInitializable
{
    [SerializeField] protected NavVisualizer visualizer;
    public EditorState EditorState { get; set; }

    protected virtual void Start()
    {
        if (visualizer == null) visualizer = FindFirstObjectByType<NavVisualizer>();
        visualizer.gameObject.SetActive(true);
    }

    public abstract void Init();
}
