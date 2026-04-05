using UnityEngine;
using UnityEngine.UI;

public class LayeredParallax : MonoBehaviour
{
    public Vector2 referenceVelocity;

    [SerializeField] Parallax[] layers;
    [SerializeField] float[] layerDistances;
    [SerializeField] float speedFactor;

    [SerializeField] bool useFixedUpdate;

    private void Start()
    {
        if (layers.Length != layerDistances.Length) return;

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].useFixedUpdate = useFixedUpdate;
        }
    }

    private void Update()
    {
        if (!useFixedUpdate)
            UpdateLayers();
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
            UpdateLayers();
    }

    void UpdateLayers()
    {
        if (layers.Length != layerDistances.Length) return;

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].x = referenceVelocity.x * (1 / layerDistances[i] * speedFactor);
            layers[i].y = referenceVelocity.y * (1 / layerDistances[i] * speedFactor);
        }
    }
}
