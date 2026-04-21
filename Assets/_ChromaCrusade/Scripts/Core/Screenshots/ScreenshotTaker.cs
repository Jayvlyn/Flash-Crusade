using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    static int count = 1;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Screenshot Taken");
            // Saves to the project root folder in Editor or data folder in builds
            ScreenCapture.CaptureScreenshot($"Screenshot{count}.png");
        }
    }
}
