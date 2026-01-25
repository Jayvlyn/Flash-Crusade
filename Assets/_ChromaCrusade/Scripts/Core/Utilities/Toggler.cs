using UnityEngine;

public class Toggler : MonoBehaviour
{
    [SerializeField] GameObject objectsToToggle;

    public void Toggle()
    {
        if (objectsToToggle == null) return;
        
        objectsToToggle.SetActive(!objectsToToggle.activeSelf);
    }
}
