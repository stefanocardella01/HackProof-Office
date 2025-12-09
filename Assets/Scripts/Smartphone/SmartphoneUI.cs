using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestisce la UI dello smartphone aziendale.
/// Mostra la lista dei messaggi e il dettaglio del messaggio selezionato.
/// Include animazione slide up/down per apertura/chiusura.
/// </summary>
public class SmartphoneUI : MonoBehaviour
{
    [Header("Pannelli Principali")]
    [SerializeField] private GameObject smartphonePanel;        // Pannello principale dello smartphone
    [SerializeField] private RectTransform smartphoneRect;      // RectTransform per animazione
    [SerializeField] private GameObject messageListPanel;       // Pannello lista messaggi
    [SerializeField] private GameObject messageDetailPanel;     // Pannello dettaglio messaggio

    [Header("Animazione Slide")]
    [SerializeField] private float hiddenYPosition = -400f;     // Posizione Y quando nascosto (abbassato)
    [SerializeField] private float visibleYPosition = 0f;       // Posizione Y quando visibile (alzato)
    [SerializeField] private float animationDuration = 0.3f;    // Durata animazione in secondi
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curva di easing

    [Header("Notifica (quando smartphone chiuso)")]
    [SerializeField] private GameObject notificationBadge;      // Badge notifica (pallino rosso)
    [SerializeField] private TextMeshProUGUI notificationCountText; // Numero messaggi non letti

    [Header("Lista Messaggi")]
    [SerializeField] private Transform messageListContent;      // Parent per gli item della lista
    [SerializeField] private GameObject messageItemPrefab;      // Prefab per singolo item messaggio

    [Header("Dettaglio Messaggio")]
    [SerializeField] private Image senderIconImage;             // Icona mittente
    [SerializeField] private TextMeshProUGUI senderNameText;    // Nome mittente
    [SerializeField] private TextMeshProUGUI timestampText;     // Orario
    [SerializeField] private TextMeshProUGUI messageBodyText;   // Corpo del messaggio
    [SerializeField] private Button backButton;                 // Bottone per tornare alla lista

    [Header("Orologio")]
    [SerializeField] private TextMeshProUGUI clockTextClosed;   // Orologio visibile quando smartphone CHIUSO
    [SerializeField] private TextMeshProUGUI clockTextOpen;     // Orologio visibile quando smartphone APERTO 

    [Header("Riferimenti Altri Canvas")]
    [SerializeField] private GameObject hudCanvas;              // Canvas HUD da nascondere

    private SmartphoneManager manager;
    private SmartphoneMessage currentMessage;                   // Messaggio attualmente visualizzato
    private List<GameObject> spawnedMessageItems = new List<GameObject>();

    // Stato animazione
    private Coroutine currentAnimation;
    private bool isAnimating = false;

    private void Awake()
    {
        // Setup iniziale: smartphone visibile ma abbassato (posizione nascosta)
        if (smartphonePanel != null)
        {
            smartphonePanel.SetActive(true);  // Sempre attivo per poter animare

            // Nascondi i pannelli interni
            if (messageListPanel != null)
                messageListPanel.SetActive(false);
            if (messageDetailPanel != null)
                messageDetailPanel.SetActive(false);
        }

        // Posiziona lo smartphone in basso (nascosto)
        if (smartphoneRect != null)
        {
            Vector2 pos = smartphoneRect.anchoredPosition;
            pos.y = hiddenYPosition;
            smartphoneRect.anchoredPosition = pos;
        }

        if (notificationBadge != null)
            notificationBadge.SetActive(false);

        // Setup orologi: all'inizio mostra solo quello per "chiuso"
        if (clockTextClosed != null)
            clockTextClosed.gameObject.SetActive(true);
        if (clockTextOpen != null)
            clockTextOpen.gameObject.SetActive(false);
    }

    private void Start()
    {
        manager = SmartphoneManager.Instance;

        if (manager == null)
        {
            Debug.LogError("[SmartphoneUI] SmartphoneManager non trovato!");
            return;
        }

        // Iscrizione agli eventi
        manager.OnSmartphoneOpened += HandleSmartphoneOpened;
        manager.OnSmartphoneClosed += HandleSmartphoneClosed;
        manager.OnMessageReceived += HandleMessageReceived;
        manager.OnMessageRead += HandleMessageRead;

        // Setup back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(ShowMessageList);
        }

        // Auto-find RectTransform se non assegnato
        if (smartphoneRect == null && smartphonePanel != null)
        {
            smartphoneRect = smartphonePanel.GetComponent<RectTransform>();
        }

        // Aggiorna la UI iniziale
        UpdateNotificationBadge();
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.OnSmartphoneOpened -= HandleSmartphoneOpened;
            manager.OnSmartphoneClosed -= HandleSmartphoneClosed;
            manager.OnMessageReceived -= HandleMessageReceived;
            manager.OnMessageRead -= HandleMessageRead;
        }
    }

    private void Update()
    {
        // Aggiorna entrambi gli orologi dello smartphone
        string currentTime = System.DateTime.Now.ToString("HH:mm");

        if (clockTextClosed != null)
            clockTextClosed.text = currentTime;

        if (clockTextOpen != null)
            clockTextOpen.text = currentTime;
    }

    #region Animation

    /// <summary>
    /// Anima lo smartphone verso l'alto (apertura).
    /// </summary>
    private void SlideUp()
    {
        if (isAnimating)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(AnimateSlide(visibleYPosition));
    }

    /// <summary>
    /// Anima lo smartphone verso il basso (chiusura).
    /// </summary>
    private void SlideDown()
    {
        if (isAnimating)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(AnimateSlide(hiddenYPosition));
    }

    /// <summary>
    /// Coroutine per animare lo slide dello smartphone.
    /// </summary>
    private IEnumerator AnimateSlide(float targetY)
    {
        if (smartphoneRect == null) yield break;

        isAnimating = true;

        Vector2 startPos = smartphoneRect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, targetY);

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;  
            float t = elapsed / animationDuration;
            float curveValue = slideCurve.Evaluate(t);

            smartphoneRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

            yield return null;
        }

        // Assicurati di arrivare esattamente alla posizione finale
        smartphoneRect.anchoredPosition = endPos;
        isAnimating = false;
    }

    #endregion

    #region Event Handlers

    private void HandleSmartphoneOpened()
    {
        // Mostra i pannelli interni PRIMA dell'animazione
        if (messageListPanel != null)
            messageListPanel.SetActive(true);
        if (messageDetailPanel != null)
            messageDetailPanel.SetActive(false);

        // Nascondi HUD principale
        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Scambia orologi: mostra quello "aperto", nascondi quello "chiuso"
        if (clockTextClosed != null)
            clockTextClosed.gameObject.SetActive(false);
        if (clockTextOpen != null)
            clockTextOpen.gameObject.SetActive(true);

        // Mostra la lista messaggi
        ShowMessageList();

        // Aggiorna la lista
        RefreshMessageList();

        // Avvia animazione slide up
        SlideUp();
    }

    private void HandleSmartphoneClosed()
    {
        // Avvia animazione slide down
        SlideDown();

        // Nascondi i pannelli interni dopo un piccolo delay (durante l'animazione)
        StartCoroutine(HidePanelsAfterDelay());

        // Ripristina HUD principale
        if (hudCanvas != null)
            hudCanvas.SetActive(true);

        // Scambia orologi: mostra quello "chiuso", nascondi quello "aperto"
        if (clockTextClosed != null)
            clockTextClosed.gameObject.SetActive(true);
        if (clockTextOpen != null)
            clockTextOpen.gameObject.SetActive(false);

        currentMessage = null;
    }

    /// <summary>
    /// Nasconde i pannelli interni dopo che l'animazione è completata.
    /// </summary>
    private IEnumerator HidePanelsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(animationDuration);

        if (!manager.IsOpen)  // Verifica che sia ancora chiuso
        {
            if (messageListPanel != null)
                messageListPanel.SetActive(false);
            if (messageDetailPanel != null)
                messageDetailPanel.SetActive(false);
        }
    }

    private void HandleMessageReceived(SmartphoneMessage message)
    {
        UpdateNotificationBadge();

        // Se lo smartphone è aperto, aggiorna la lista
        if (manager.IsOpen)
        {
            RefreshMessageList();
        }
    }

    private void HandleMessageRead(SmartphoneMessage message)
    {
        UpdateNotificationBadge();
        RefreshMessageList();

        // Se c'è un target da evidenziare, attiva l'outline
        if (message.targetToHighlight != null)
        {
            HighlightTarget(message.targetToHighlight);
        }
    }

    #endregion

    #region UI Navigation

    /// <summary>
    /// Mostra il pannello lista messaggi.
    /// </summary>
    public void ShowMessageList()
    {
        if (messageListPanel != null)
            messageListPanel.SetActive(true);

        if (messageDetailPanel != null)
            messageDetailPanel.SetActive(false);

        manager?.PlayButtonClick();
    }

    /// <summary>
    /// Mostra il dettaglio di un messaggio specifico.
    /// </summary>
    public void ShowMessageDetail(SmartphoneMessage message)
    {
        if (message == null) return;

        currentMessage = message;

        if (messageListPanel != null)
            messageListPanel.SetActive(false);

        if (messageDetailPanel != null)
            messageDetailPanel.SetActive(true);

        // Popola i campi
        if (senderNameText != null)
            senderNameText.text = message.senderName;

        if (timestampText != null)
            timestampText.text = message.timestamp;

        if (messageBodyText != null)
            messageBodyText.text = message.messageText;

        if (senderIconImage != null)
        {
            if (message.senderIcon != null)
            {
                senderIconImage.sprite = message.senderIcon;
                senderIconImage.gameObject.SetActive(true);
            }
            else
            {
                senderIconImage.gameObject.SetActive(false);
            }
        }

        // Segna come letto
        manager?.MarkAsRead(message);
        manager?.PlayButtonClick();
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// Aggiorna il badge delle notifiche.
    /// </summary>
    private void UpdateNotificationBadge()
    {
        if (manager == null) return;

        int unreadCount = manager.UnreadCount;

        if (notificationBadge != null)
        {
            notificationBadge.SetActive(unreadCount > 0);
        }

        if (notificationCountText != null)
        {
            notificationCountText.text = unreadCount > 9 ? "9+" : unreadCount.ToString();
        }
    }

    /// <summary>
    /// Rigenera la lista dei messaggi.
    /// </summary>
    private void RefreshMessageList()
    {
        if (manager == null || messageListContent == null) return;

        // Pulisci gli item esistenti
        foreach (var item in spawnedMessageItems)
        {
            if (item != null)
                Destroy(item);
        }
        spawnedMessageItems.Clear();

        // Se non abbiamo un prefab, non possiamo creare gli item
        if (messageItemPrefab == null)
        {
            Debug.LogWarning("[SmartphoneUI] messageItemPrefab non assegnato!");
            return;
        }

        // Crea gli item per ogni messaggio
        var messages = manager.GetAllMessages();
        foreach (var message in messages)
        {
            GameObject item = Instantiate(messageItemPrefab, messageListContent);
            spawnedMessageItems.Add(item);

            // Configura l'item
            var itemUI = item.GetComponent<SmartphoneMessageItem>();
            if (itemUI != null)
            {
                itemUI.Setup(message, this);
            }
            else
            {
                // Fallback: cerca i componenti manualmente
                SetupMessageItemManually(item, message);
            }
        }
    }

    /// <summary>
    /// Setup manuale per item senza script SmartphoneMessageItem.
    /// </summary>
    private void SetupMessageItemManually(GameObject item, SmartphoneMessage message)
    {
        // Cerca il testo del mittente
        var senderText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (senderText != null)
        {
            string prefix = message.isRead ? "" : "● ";
            senderText.text = $"{prefix}{message.senderName}";
        }

        // Aggiungi click handler
        var button = item.GetComponent<Button>();
        if (button == null)
        {
            button = item.AddComponent<Button>();
        }

        // Crea una copia locale per la closure
        var msg = message;
        button.onClick.AddListener(() => ShowMessageDetail(msg));
    }

    /// <summary>
    /// Evidenzia un target (es: NPC collega).
    /// Puoi implementare qui la logica di outline shader.
    /// </summary>
    private void HighlightTarget(GameObject target)
    {
        if (target == null) return;

        Debug.Log($"[SmartphoneUI] Evidenziando target: {target.name}");

        // TODO: Implementa la logica di outline
        // Esempio: target.GetComponent<OutlineEffect>()?.Enable();

        // Per ora, se c'è un componente Renderer, cambiamo temporaneamente il colore
        var renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Potresti usare un shader di outline qui
            Debug.Log($"[SmartphoneUI] Target {target.name} ha un Renderer - implementa outline shader");
        }
    }

    #endregion

    #region Public Getters

    /// <summary>
    /// Restituisce true se l'animazione è in corso.
    /// </summary>
    public bool IsAnimating => isAnimating;

    #endregion
}