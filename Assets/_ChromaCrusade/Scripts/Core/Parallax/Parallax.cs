using UnityEngine;
using UnityEngine.UI;

public class Parallax : MonoBehaviour
{
    [SerializeField] private RawImage img;
    public float x, y;
    public bool useFixedUpdate;

    private void Update()
    {
        if (!useFixedUpdate)
            UpdateRect();
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
            UpdateRect();
    }

    void UpdateRect()
    {
        img.uvRect = new Rect(img.uvRect.position + new Vector2(x, y) * Time.deltaTime, img.uvRect.size);
    }
}
