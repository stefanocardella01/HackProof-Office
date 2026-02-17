using UnityEngine;

public class ScreenshotTool : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png", 4);
        }
    }
}
