using UnityEngine;

[System.Serializable]
public class ReportEntry
{
    public string label;                // testo breve (titolo riga)
    public ReportCheck check;           // quale check leggere

    [Tooltip("Valore considerato 'corretto' per questo check. Di default true.")]
    public bool expectedValue = true;

    [TextArea] public string okText;    // spiegazione se giusto
    [TextArea] public string badText;   // spiegazione se sbagliato
}
