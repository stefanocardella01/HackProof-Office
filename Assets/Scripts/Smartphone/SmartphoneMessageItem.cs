using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script per un singolo item nella lista messaggi dello smartphone.
/// Attaccato al prefab del messaggio.
/// </summary>
public class SmartphoneMessageItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image senderIcon;
    [SerializeField] private TextMeshProUGUI senderNameText;
    [SerializeField] private TextMeshProUGUI previewText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private GameObject unreadIndicator;  // Pallino per messaggio non letto
    [SerializeField] private Image backgroundImage;

    [Header("Colori")]
    [SerializeField] private Color unreadBackgroundColor = new Color(0.9f, 0.95f, 1f);
    [SerializeField] private Color readBackgroundColor = Color.white;

    private SmartphoneMessage message;
    private SmartphoneUI smartphoneUI;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// Configura l'item con i dati del messaggio.
    /// </summary>
    public void Setup(SmartphoneMessage msg, SmartphoneUI ui)
    {
        message = msg;
        smartphoneUI = ui;

        // Popola i campi UI
        if (senderNameText != null)
        {
            senderNameText.text = msg.senderName;
            // Grassetto se non letto
            senderNameText.fontStyle = msg.isRead ? FontStyles.Normal : FontStyles.Bold;
        }

        if (previewText != null)
        {
            // Mostra solo i primi 50 caratteri come anteprima
            string preview = msg.messageText;
            if (preview.Length > 50)
            {
                preview = preview.Substring(0, 47) + "...";
            }
            previewText.text = preview;
        }

        if (timestampText != null)
        {
            timestampText.text = msg.timestamp;
        }

        if (senderIcon != null)
        {
            if (msg.senderIcon != null)
            {
                senderIcon.sprite = msg.senderIcon;
                senderIcon.gameObject.SetActive(true);
            }
            senderIcon.gameObject.SetActive(true);
            
        }

        // Indicatore non letto
        if (unreadIndicator != null)
        {
            unreadIndicator.SetActive(!msg.isRead);
        }

        // Colore sfondo
        if (backgroundImage != null)
        {
            backgroundImage.color = msg.isRead ? readBackgroundColor : unreadBackgroundColor;
        }
    }

    /// <summary>
    /// Gestisce il click sull'item.
    /// </summary>
    private void OnClick()
    {
        if (smartphoneUI != null && message != null)
        {
            smartphoneUI.ShowMessageDetail(message);
        }
    }

    /// <summary>
    /// Aggiorna lo stato visivo (chiamato dopo che il messaggio è stato letto).
    /// </summary>
    public void Refresh()
    {
        if (message != null)
        {
            Setup(message, smartphoneUI);
        }
    }
}