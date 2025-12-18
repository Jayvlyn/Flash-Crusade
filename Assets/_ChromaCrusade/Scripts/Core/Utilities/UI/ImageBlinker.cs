using UnityEngine;
using UnityEngine.UI;

public class ImageBlinker : MonoBehaviour
{
    public Color color1 = Color.white;
    public Color color2 = Color.red;
    public float blinkRate;

    private Image image;
    private float blinkTimer;

    private void Awake()
    {
        image = GetComponent<Image>();
        blinkTimer = blinkRate;
    }

    private void Update()
    {
        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0)
        {
            blinkTimer = blinkRate;
            ToggleColor();
        }
    }

    private void ToggleColor()
    {
        image.color = image.color.Equals(color1) ? color2 : color1;
    }
}
