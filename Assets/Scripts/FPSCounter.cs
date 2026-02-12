using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private float timer = 0f;
    private float minFPS;
    private float maxFPS;
    private float currentFPS;

    void Start()
    {
        ResetMinMax();
    }

    void Update()
    {
        currentFPS = 1f / Time.unscaledDeltaTime;

        // Ignora valori impossibili
        if (currentFPS > 0 && currentFPS < 1000)
        {
            if (currentFPS < minFPS) minFPS = currentFPS;
            if (currentFPS > maxFPS) maxFPS = currentFPS;
        }

        timer += Time.unscaledDeltaTime;

        // Reset ogni secondo (come fanno molti giochi)
        if (timer >= 1f)
        {
            timer = 0f;
            ResetMinMax();
        }

        fpsText.text = $"{Mathf.Round(currentFPS)} FPS ({Mathf.Round(minFPS)} | {Mathf.Round(maxFPS)})";
    }

    void ResetMinMax()
    {
        minFPS = float.MaxValue;
        maxFPS = 0f;
    }
}