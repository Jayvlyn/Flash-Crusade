using UnityEngine;

public abstract class Navigator : MonoBehaviour, IInitializable
{
    [SerializeField] protected EditorNavVisualizer visualizer;
    public EditorState EditorState { get; set; }

    protected virtual void Start()
    {
        if (visualizer == null) visualizer = FindFirstObjectByType<EditorNavVisualizer>();
        visualizer.gameObject.SetActive(true);
    }

    public abstract void Init();
}
