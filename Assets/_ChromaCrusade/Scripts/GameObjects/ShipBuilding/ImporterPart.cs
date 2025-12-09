using UnityEngine;
using UnityEngine.UI;

public class ImporterPart : MonoBehaviour
{
    public Image image;

    [Header("IMPORT CONFIG")]
    public Sprite partSprite;


    private void Awake()
    {
        if (partSprite != null)
            image.sprite = partSprite;
        else
            image.enabled = false;
    }

    private void OnValidate()
    {
        if (image == null) return;

        if (partSprite != null)
        {
            image.sprite = partSprite;
            image.enabled = true;
        }
        else
            image.enabled = false;
    }
}
