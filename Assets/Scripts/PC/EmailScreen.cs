using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmailScreen : MonoBehaviour
{
    [Header("UI Riferimenti")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image emailImageDisplay;
    [SerializeField] private GameObject emailContentContainer;

    [Header("Sistema Hover Manuale")]
    // Trascina qui i tuoi 5 oggetti "Hotspot_EmailX" dalla scena
    [SerializeField] private UrlHoverArea[] manualHotspots;

    [Header("Tooltip Fisso")]
    [SerializeField] private GameObject tooltipPanel; // Il pannello grigio scuro (fisso)
    [SerializeField] private TextMeshProUGUI tooltipText; // Il testo dentro il pannello

    [Header("Pulsanti")]
    [SerializeField] private Button phishingButton;
    [SerializeField] private Button legitimateButton;

    private EmailInterfaceManager manager;

    public void Initialize(EmailInterfaceManager interfaceManager)
    {
        manager = interfaceManager;

        if (phishingButton != null)
            phishingButton.onClick.AddListener(() => manager.OnPlayerChoice(EmailType.Phishing));

        if (legitimateButton != null)
            legitimateButton.onClick.AddListener(() => manager.OnPlayerChoice(EmailType.Legitimate));

        // Nascondi il tooltip all'avvio
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    public void ShowEmail(EmailData email, int currentIndex, int totalEmails)
    {
        if (emailContentContainer != null) emailContentContainer.SetActive(true);

        // 1. Mostra immagine
        if (emailImageDisplay != null)
        {
            emailImageDisplay.sprite = email.emailImage;
            emailImageDisplay.preserveAspect = true;
        }

        if (progressText != null)
            progressText.text = $"Email {currentIndex} di {totalEmails}";

        // 2. GESTIONE HOTSPOTS
        // Disattiva TUTTI gli hotspot
        if (manualHotspots != null)
        {
            foreach (var spot in manualHotspots)
            {
                if (spot != null) spot.gameObject.SetActive(false);
            }

            // Calcola l'indice reale (0-4)
            int realIndex = currentIndex - 1;

            // Attiva SOLO l'hotspot per questa email
            if (realIndex >= 0 && realIndex < manualHotspots.Length)
            {
                var activeSpot = manualHotspots[realIndex];
                if (activeSpot != null)
                {
                    activeSpot.gameObject.SetActive(true);
                    // Passa il testo URL specifico di questa email
                    activeSpot.Configure(email.visibleUrl, tooltipPanel, tooltipText);
                }
            }
        }
    }

    public void Hide()
    {
        if (emailContentContainer != null) emailContentContainer.SetActive(false);
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}