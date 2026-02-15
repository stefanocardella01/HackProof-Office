using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ReportRowUI : MonoBehaviour
{
    [Header("Testi")]
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI explanationText;

    [Header("Sfondo riga")]
    public Image rowBackground;

    public Color okColor = new Color(0.55f, 0.78f, 0.65f, 0.90f);     // verde tenue
    public Color badColor = new Color(0.82f, 0.55f, 0.55f, 0.90f);    // rosso tenue
    public Color minorBadColor = new Color(0.95f, 0.88f, 0.55f, 0.90f); // giallo tenue

    // Oggetti meno gravi
    private static readonly HashSet<ReportCheck> minorChecks = new()
    {
        ReportCheck.ManualDelivered,
        ReportCheck.ScrewdriverDelivered
    };

    //NUOVA versione che riceve anche il check
    public void Setup(ReportCheck check, string label, bool ok, string explanation)
    {
        labelText.text = label;
        explanationText.text = explanation;

        if (rowBackground != null)
        {
            if (ok)
            {
                rowBackground.color = okColor;
            }
            else
            {
                rowBackground.color = minorChecks.Contains(check)
                    ? minorBadColor
                    : badColor;
            }
        }
    }

    public void Setup(string label, bool ok, string explanation)
    {
        Setup(default, label, ok, explanation);
    }
}
