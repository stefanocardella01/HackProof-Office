using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ChecklistItemUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private Image checkIcon;
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Icone")]
    [SerializeField] private Sprite uncheckedIcon;   
    [SerializeField] private Sprite checkedIcon;     

    [Header("Colori")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    [SerializeField] private Color counterColor = new Color(1f, 0.85f, 0.4f, 1f); 

    [Header("Strikethrough")]
    [SerializeField] private bool useStrikethrough = true;

    private MissionObjective linkedObjective;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// Inizializza la riga con un obiettivo.
    /// </summary>
    public void Setup(MissionObjective objective)
    {
        linkedObjective = objective;
        UpdateVisual();
    }

    /// <summary>
    /// Aggiorna l'aspetto visivo in base allo stato dell'obiettivo.
    /// </summary>
    public void UpdateVisual()
    {
        if (linkedObjective == null) return;

        // Testo
        if (objectiveText != null)
        {
            string text = linkedObjective.GetDisplayText();

            if (linkedObjective.IsCompleted && useStrikethrough)
            {
                objectiveText.text = $"<s>{text}</s>";
                objectiveText.color = completedColor;
            }
            else
            {
                objectiveText.text = text;

                // Colore speciale per contatori attivi (es: 1/3, 2/3)
                if (linkedObjective.ShowCounter && linkedObjective.CurrentCount > 0 && !linkedObjective.IsCompleted)
                    objectiveText.color = counterColor;
                else
                    objectiveText.color = normalColor;
            }
        }

        // Icona
        if (checkIcon != null)
        {
            if (linkedObjective.IsCompleted)
            {
                checkIcon.sprite = checkedIcon;
                checkIcon.color = completedColor;
            }
            else
            {
                checkIcon.sprite = uncheckedIcon;
                checkIcon.color = normalColor;
            }
        }
    }

    /// <summary>
    /// Restituisce l'ID dell'obiettivo collegato.
    /// </summary>
    public string GetObjectiveId()
    {
        return linkedObjective?.ObjectiveId;
    }
}
