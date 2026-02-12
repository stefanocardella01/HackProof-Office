using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReportRowUI : MonoBehaviour
{
 

    [Header("Testi")]
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI explanationText;

    [Header("Sfondo riga")]
    public Image rowBackground;  // <-- AGGIUNGI QUESTO

    public Color okColor = new Color(0.65f, 0.85f, 0.65f, 0.85f);   // verde tenue
    public Color badColor = new Color(0.90f, 0.65f, 0.65f, 0.85f);   // rosso tenue

    public void Setup(string label, bool ok, string explanation)
    {
        labelText.text = label;
        explanationText.text = explanation;


        if (rowBackground != null)
            rowBackground.color = ok ? okColor : badColor;   // <-- QUESTA È LA MODIFICA
    }
}
