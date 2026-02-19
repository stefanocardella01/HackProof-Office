using UnityEngine;

public class ScreenshotTool : MonoBehaviour
{

    private int count;

    private void Start()
    {

        count = 0;

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ScreenCapture.CaptureScreenshot("Screenshot" + count + ".png");

            count++;
        }
    }
}
