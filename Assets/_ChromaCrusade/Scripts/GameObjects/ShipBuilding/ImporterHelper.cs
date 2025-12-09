using UnityEngine;
using UnityEngine.UI;

public class ImporterHelper : MonoBehaviour
{
    public Image image;
    public ImporterPart part;
    public ImporterSegment[] segments;

    private void Awake()
    {
        part.image = image;

        for (int i = 0; i < segments.Length; i++)
        {
            part.segments = new ImporterSegment[segments.Length];
            part.segments[i] = segments[i];
        }
    }
}
