using UnityEngine;
using UnityEngine.UI;

public class LayeredParallax : MonoBehaviour
{
    public Vector2 referenceVelocity;

    [SerializeField] private Parallax[] layers;
    [SerializeField] private float[] layerDistances;
    [SerializeField] private float speedFactor;

    private void Update()
    {
        if (layers.Length != layerDistances.Length) return;

        for(int i = 0; i < layers.Length; i++)
        {
            layers[i].x = referenceVelocity.x * (1/layerDistances[i] * speedFactor);
            layers[i].y = referenceVelocity.y * (1/layerDistances[i] * speedFactor);
        }
    }
}
