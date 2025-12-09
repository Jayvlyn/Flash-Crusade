using UnityEngine;
using UnityEngine.UI;

public class ImporterHelper : MonoBehaviour
{
    public Image image;
    public ImporterPart part;

    private void Awake()
    {
        part.image = image;
    }
}
