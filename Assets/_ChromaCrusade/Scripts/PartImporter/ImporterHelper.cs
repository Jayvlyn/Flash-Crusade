#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

public class ImporterHelper : MonoBehaviour
{
    public Image image;
    public ImporterPart part;
    public ImporterSegment[] segments;

    public GameObject segmentButtonsParent;
    public RectTransform firepointPrefab;
    public Transform firepointsParent;

    private void Awake()
    {
        part.image = image;

        part.segments = new ImporterSegment[segments.Length];
        for (int i = 0; i < segments.Length; i++)
        {
            part.segments[i] = segments[i];
        }

        part.segmentButtonsParent = segmentButtonsParent;
        part.firepointPrefab = firepointPrefab;
        part.firepointsParent = firepointsParent;
    }
}
#endif
