using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestisce la visualizzazione di una singola email e i pulsanti di scelta.
/// Simile alla UI del Phishing Quiz di Google/Jigsaw.
/// </summary>
public class EmailScreen : MonoBehaviour
{
    [Header("Header - Domanda")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Pulsanti Scelta")]
    [SerializeField] private Button phishingButton;
    [SerializeField] private Button legitimateButton;

    [Header("Email Preview")]
    [SerializeField] private TextMeshProUGUI senderNameText;
    [SerializeField] private TextMeshProUGUI senderEmailText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private TextMeshProUGUI subjectText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("URL")]
    [SerializeField] private TextMeshProUGUI urlText;  // URL sempre visibile

    [Header("Container Principale")]
    [SerializeField] private GameObject emailContentContainer;

    [Header("Testi Personalizzabili")]
    [SerializeField] private string questionTemplate = "Questa email è legittima o è un tentativo di phishing?";
    [SerializeField] private string hintTemplate = "Analizza attentamente mittente, contenuto e link.";

    // Riferimenti
    private EmailInterfaceManager manager;
    private EmailData currentEmail;

    public void Initialize(EmailInterfaceManager interfaceManager)
    {
        manager = interfaceManager;

        // Setup pulsanti
        if (phishingButton != null)
            phishingButton.onClick.AddListener(OnPhishingClicked);

        if (legitimateButton != null)
            legitimateButton.onClick.AddListener(OnLegitimateClicked);
    }

    /// <summary>
    /// Mostra un'email specifica
    /// </summary>
    public void ShowEmail(EmailData email, int currentIndex, int totalEmails)
    {
        currentEmail = email;

        // Mostra contenuto email
        if (emailContentContainer != null)
            emailContentContainer.SetActive(true);

        // Abilita pulsanti di scelta
        if (phishingButton != null)
            phishingButton.interactable = true;

        if (legitimateButton != null)
            legitimateButton.interactable = true;

        // Popola header
        if (questionText != null)
            questionText.text = questionTemplate;

        if (hintText != null)
            hintText.text = hintTemplate;

        if (progressText != null)
            progressText.text = $"Email {currentIndex} di {totalEmails}";

        // Popola email preview
        if (senderNameText != null)
            senderNameText.text = email.senderName;

        if (senderEmailText != null)
            senderEmailText.text = $"<{email.senderEmail}>";

        if (timestampText != null)
        {
            // Genera un timestamp fittizio
            int hour = Random.Range(8, 19);
            int minute = Random.Range(0, 59);
            timestampText.text = $"{hour:D2}:{minute:D2}";
        }

        if (subjectText != null)
            subjectText.text = email.subject;

        if (bodyText != null)
            bodyText.text = email.body;

        // Mostra URL direttamente
        if (urlText != null)
            urlText.text = email.url;

        Debug.Log($"[EmailScreen] Mostrando email: {email.subject}");
    }

    #region Button Handlers

    private void OnPhishingClicked()
    {
        Debug.Log("[EmailScreen] Pulsante PHISHING cliccato");

        if (manager != null)
            manager.OnPlayerChoice(EmailType.Phishing);
    }

    private void OnLegitimateClicked()
    {
        Debug.Log("[EmailScreen] Pulsante LEGITTIMO cliccato");

        if (manager != null)
            manager.OnPlayerChoice(EmailType.Legitimate);
    }

    #endregion

    /// <summary>
    /// Nasconde la schermata email
    /// </summary>
    public void Hide()
    {
        if (emailContentContainer != null)
            emailContentContainer.SetActive(false);
    }
}