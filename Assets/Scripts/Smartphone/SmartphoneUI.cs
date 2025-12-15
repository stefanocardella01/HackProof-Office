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
    [SerializeField] private GameObject homePanel;              // Pannello Home con icone

    [Header("Animazione Slide")]
    [SerializeField] private float hiddenYPosition = -520f;     // Posizione Y quando nascosto (abbassato)
    [SerializeField] private float visibleYPosition = 0f;       // Posizione Y quando visibile (alzato)
    [SerializeField] private float animationDuration = 0.3f;    // Durata animazione in secondi
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curva di easing

    [Header("Banner Notifica")]
    [SerializeField] private GameObject notificationBanner;     // Banner "X nuove notifiche"
    [SerializeField] private TextMeshProUGUI notificationBannerText; // Testo del banner

    [Header("Lista Messaggi")]
    [SerializeField] private Transform messageListContent;      // Parent per gli item della lista
    [SerializeField] private GameObject messageItemPrefab;      // Prefab per singolo item messaggio
    [SerializeField] private Button backToHomeButton;           // Bottone per tornare alla Home

    [Header("Home Panel")]
    [SerializeField] private Button messagesButton;             // Bottone per aprire messaggi
    [SerializeField] private Button phoneButton;                // Bottone telefono (non fa niente)
    [SerializeField] private GameObject homeBadge;              // Badge notifiche sull'icona messaggi
    [SerializeField] private TextMeshProUGUI homeBadgeText;     // Testo del badge (numero)

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
            if (homePanel != null)
                homePanel.SetActive(false);
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

        // Nascondi il banner notifica all'inizio
        if (notificationBanner != null)
            notificationBanner.SetActive(false);

        // Nascondi il badge sulla home all'inizio
        if (homeBadge != null)
            homeBadge.SetActive(false);

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

        // Setup back to home button (da MessageList a Home)
        if (backToHomeButton != null)
        {
            backToHomeButton.onClick.AddListener(ShowHome);
        }
        //setup messages button (da Home a MessageList)
        if (messagesButton != null)
        {
            messagesButton.onClick.AddListener(ShowMessageListFromHome);
        }

        // Auto-find RectTransform se non assegnato
        if (smartphoneRect == null && smartphonePanel != null)
        {
            smartphoneRect = smartphonePanel.GetComponent<RectTransform>();
        }

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
        currentAnimation = StartCoroutine(AnimateSlideUp(visibleYPosition));
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
        currentAnimation = StartCoroutine(AnimateSlideDown());
    }

    /// <summary>
    /// Coroutine per animare lo slide up dello smartphone.
    /// </summary>
    private IEnumerator AnimateSlideUp(float targetY)
    {
        if (smartphoneRect == null) yield break;

        isAnimating = true;

        Vector2 startPos = smartphoneRect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, targetY);

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;  
            float t = elapsed / animationDuration;
            float curveValue = slideCurve.Evaluate(t);

            smartphoneRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

            yield return null;
        }

        // Assicurati di arrivare esattamente alla posizione finale
        smartphoneRect.anchoredPosition = endPos;
        isAnimating = false;
        
        //Dopo l'animazione: mostra l'orologio open
        if (clockTextOpen != null)
            clockTextOpen.gameObject.SetActive(true);

        // DOPO l'animazione mostra la home
        ShowHome();
    }


    /// <summary>
    /// Coroutine per animare lo slide DOWN (chiusura).
    /// I pannelli sono già nascosti PRIMA dell'animazione.
    /// </summary>
    private IEnumerator AnimateSlideDown()
    {
        if (smartphoneRect == null) yield break;

        isAnimating = true;
       

        Vector2 startPos = smartphoneRect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, hiddenYPosition);

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            float curveValue = slideCurve.Evaluate(t);

            smartphoneRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

            yield return null;
        }

        // Assicurati di arrivare esattamente alla posizione finale
        smartphoneRect.anchoredPosition = endPos;
        isAnimating = false;

        // DOPO l'animazione: mostra l'orologio closed
        if (clockTextClosed != null)
            clockTextClosed.gameObject.SetActive(true);
        
    }

    #endregion

    #region Event Handlers

    private void HandleSmartphoneOpened()
    {
        // Nascondi SUBITO il banner quando si apre lo smartphone
        if (notificationBanner != null)
            notificationBanner.SetActive(false);

        // Nascondi HUD principale
        if (hudCanvas != null)
            hudCanvas.SetActive(false);

        // Nascondi l'orologio chiuso SUBITO (quello aperto apparirà DOPO l'animazione)
        if (clockTextClosed != null)
            clockTextClosed.gameObject.SetActive(false);

        // NON mostrare i pannelli qui - appariranno DOPO l'animazione in AnimateSlideUp()

        // Avvia animazione slide up
        SlideUp();
    }

    private void HandleSmartphoneClosed()
    {
        // Nascondi tutti i pannelli SUBITO (prima dell'animazione)
        if (notificationBanner != null)
            notificationBanner.SetActive(false); // Nascondi il banner durante l'animazione
        if (homePanel != null)
            homePanel.SetActive(false);
        if (messageListPanel != null)
            messageListPanel.SetActive(false);
        if (messageDetailPanel != null)
            messageDetailPanel.SetActive(false);


        // Nascondi l'orologio aperto SUBITO (quello chiuso apparirà DOPO l'animazione)
        if (clockTextOpen != null)
            clockTextOpen.gameObject.SetActive(false);

        // Avvia animazione slide down
        SlideDown();

        // Ripristina HUD principale
        if (hudCanvas != null)
            hudCanvas.SetActive(true);

        // Aggiorna il banner (verrà mostrato se ci sono messaggi non letti)
        UpdateNotificationBanner();

        currentMessage = null;
    }
    
    private void HandleMessageReceived(SmartphoneMessage message)
    {
        // Aggiorna il banner (mostrerà notifica solo se smartphone chiuso)
        UpdateNotificationBanner();

        // Aggiorna il badge sulla home
        UpdateHomeBadge();

        // Se lo smartphone è aperto, aggiorna la lista
        if (manager.IsOpen)
        {
            RefreshMessageList();
        }
    }

    private void HandleMessageRead(SmartphoneMessage message)
    {
        // Aggiorna il banner (potrebbe nascondersi se era l'ultimo messaggio non letto)
        UpdateNotificationBanner();

        // Aggiorna il badge sulla home
        UpdateHomeBadge();

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
    /// Mostra il pannello Home.
    /// </summary>
    public void ShowHome()
    {
        if (homePanel != null)
            homePanel.SetActive(true);

        if (messageListPanel != null)
            messageListPanel.SetActive(false);

        if (messageDetailPanel != null)
            messageDetailPanel.SetActive(false);

        // Aggiorna il badge sulla home
        UpdateHomeBadge();

        manager?.PlayButtonClick();
    }

    /// <summary>
    /// Mostra il pannello lista messaggi (chiamato dalla Home).
    /// </summary>
    public void ShowMessageListFromHome()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

        if (messageListPanel != null)
            messageListPanel.SetActive(true);

        if (messageDetailPanel != null)
            messageDetailPanel.SetActive(false);

        RefreshMessageList();

        manager?.PlayButtonClick();
    }

    /// <summary>
    /// Mostra il pannello lista messaggi (chiamato dal MessageDetail).
    /// </summary>
    public void ShowMessageList()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

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

        if (homePanel != null)
            homePanel.SetActive(false);

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

        // 4. Icona mittente - mantieni visibile anche senza sprite
        if (senderIconImage != null)
        {
            if (message.senderIcon != null)
            {
                senderIconImage.sprite = message.senderIcon;
            }
            // Non nascondere l'icona, così mostra il colore placeholder
            senderIconImage.gameObject.SetActive(true);
        }

        // Segna come letto
        manager?.MarkAsRead(message);
        manager?.PlayButtonClick();
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// Aggiorna il badge sulla icona Messaggi nella Home.
    /// </summary>
    private void UpdateHomeBadge()
    {
        if (manager == null) return;

        int unreadCount = manager.UnreadCount;

        if (homeBadge != null)
        {
            homeBadge.SetActive(unreadCount > 0);
        }

        if (homeBadgeText != null && unreadCount > 0)
        {
            homeBadgeText.text = unreadCount > 9 ? "9+" : unreadCount.ToString();
        }
    }

    /// <summary>
    /// Aggiorna il banner delle notifiche.
    /// Mostra il banner SOLO se ci sono messaggi non letti E lo smartphone è CHIUSO.
    /// </summary>
    private void UpdateNotificationBanner()
    {
        if (manager == null) return;

        int unreadCount = manager.UnreadCount;
        bool shouldShowBanner = unreadCount > 0 && !manager.IsOpen;

        if (notificationBanner != null)
        {
            notificationBanner.SetActive(shouldShowBanner);
        }

        // Aggiorna il testo solo se il banner è visibile
        if (shouldShowBanner && notificationBannerText != null)
        {
            if (unreadCount == 1)
            {
                notificationBannerText.text = "1 nuova notifica";
            }
            else
            {
                notificationBannerText.text = $"{unreadCount} nuove notifiche";
            }
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