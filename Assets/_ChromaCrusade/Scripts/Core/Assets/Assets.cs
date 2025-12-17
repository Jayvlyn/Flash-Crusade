using UnityEngine;

public class Assets : MonoBehaviour
{
    private static Assets _i;

    public static Assets i
    {
        get
        {
            if (_i == null) _i = Instantiate(Resources.Load<Assets>("Assets"));
            return _i;
        }
    }

    // All references:
    public GameObject editorShipPartPrefab;
}
