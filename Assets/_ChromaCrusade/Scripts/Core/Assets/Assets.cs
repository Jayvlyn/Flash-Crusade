using UnityEngine;

public class Assets : MonoBehaviour
{
    private static Assets instance;

    public static Assets Instance
    {
        get
        {
            if (instance == null) instance = Instantiate(Resources.Load<Assets>("Assets"));
            return instance;
        }
    }

    public GameObject editorShipPartPrefab;
    public GameObject gameShipPrefab;
    public GameObject pilotPrefab;
    public RectTransform uiShipPrefab;
    public Color uiGreen;
    public Color uiRed;
    public Sprite shipSilhouette;
}
