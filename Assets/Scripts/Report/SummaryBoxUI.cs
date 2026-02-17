using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryBoxUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI groupTitleText;
    [SerializeField] private TextMeshProUGUI headlineText;
    [SerializeField] private TextMeshProUGUI linesText;

    [Header("Colors")]
    [SerializeField] private Color green = new Color(0.25f, 0.65f, 0.35f, 1f);
    [SerializeField] private Color yellow = new Color(0.95f, 0.75f, 0.20f, 1f);
    [SerializeField] private Color red = new Color(0.80f, 0.25f, 0.25f, 1f);


    public void Setup(string headline, string lines, SummaryStatus status)
    {
        if (headlineText != null) headlineText.text = headline;
        if (linesText != null) linesText.text = lines;

        if (background != null)
            background.color = status switch
            {
                SummaryStatus.Green => green,
                SummaryStatus.Yellow => yellow,
                _ => red
            };
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (background == null) background = GetComponent<Image>();
    }
#endif
}
