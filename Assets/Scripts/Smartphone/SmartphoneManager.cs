using System;
using System.Collections.Generic;
using UnityEngine;


// Manager centrale dello smartphone.
// Gestisce la lista dei messaggi, le notifiche e gli eventi.
// Pattern Singleton per accesso globale.

public class SmartphoneManager : MonoBehaviour
{
    public static SmartphoneManager Instance { get; private set; } //la possono leggere tutti ma solo il manager puo modificarla

    [Header("Messaggi")]
    [SerializeField] private List<SmartphoneMessage> messages = new List<SmartphoneMessage>();

    [Header("Audio")]
    [SerializeField] private AudioClip notificationSound;  // Suono nuova notifica
    [SerializeField] private AudioClip buttonClickSound;   // Suono click UI

    private AudioSource audioSource;

    // Eventi per comunicare con altri sistemi
    public event Action<SmartphoneMessage> OnMessageReceived;  // Nuovo messaggio arrivato
    public event Action<SmartphoneMessage> OnMessageRead;      // Messaggio letto
    public event Action OnSmartphoneOpened;                    // Smartphone aperto
    public event Action OnSmartphoneClosed;                    // Smartphone chiuso

    // Stato
    public bool IsOpen { get; private set; }
    public int UnreadCount => messages.FindAll(m => !m.isRead).Count;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Aggiunge un nuovo messaggio allo smartphone e triggera la notifica.
    /// Chiamato da altri sistemi (es: MissionManager) quando serve inviare un messaggio.
    /// </summary>
    public void ReceiveMessage(SmartphoneMessage message)
    {
        if (message == null) return;

        // Assicuriamoci che abbia un ID
        if (string.IsNullOrEmpty(message.id))
        {
            message.id = System.Guid.NewGuid().ToString();
        }

        // Imposta timestamp se non presente
        if (string.IsNullOrEmpty(message.timestamp))
        {
            message.timestamp = System.DateTime.Now.ToString("HH:mm");
        }

        messages.Insert(0, message); // Inserisci in cima (più recente)

        PlaySound(notificationSound);
        OnMessageReceived?.Invoke(message);

        Debug.Log($"[SmartphoneManager] Nuovo messaggio da {message.senderName}: {message.messageText}");
    }

    /// <summary>
    /// Overload semplificato per inviare un messaggio velocemente.
    /// </summary>
    public void ReceiveMessage(string sender, string text, GameObject targetToHighlight = null)
    {
        var message = new SmartphoneMessage(sender, text)
        {
            targetToHighlight = targetToHighlight
        };
        ReceiveMessage(message);
    }

    /// <summary>
    /// Segna un messaggio come letto.
    /// </summary>
    public void MarkAsRead(SmartphoneMessage message)
    {
        if (message == null || message.isRead) return;

        message.isRead = true;
        OnMessageRead?.Invoke(message);

        Debug.Log($"[SmartphoneManager] Messaggio letto: {message.senderName}");
    }

    /// <summary>
    /// Segna un messaggio come letto tramite ID.
    /// </summary>
    public void MarkAsRead(string messageId)
    {
        var message = messages.Find(m => m.id == messageId);
        if (message != null)
        {
            MarkAsRead(message);
        }
    }

    // Apre lo smartphone.
    public void Open()
    {
        if (IsOpen) return;

        IsOpen = true;
        OnSmartphoneOpened?.Invoke();

        Debug.Log("[SmartphoneManager] Smartphone aperto");
    }

    // Chiude lo smartphone.
    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;
        OnSmartphoneClosed?.Invoke();

        Debug.Log("[SmartphoneManager] Smartphone chiuso");
    }

    // Toggle apertura/chiusura.
    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    
    // Ottiene tutti i messaggi.
    public List<SmartphoneMessage> GetAllMessages()
    {
        return new List<SmartphoneMessage>(messages);
    }

    
    // Ottiene solo i messaggi non letti.
    public List<SmartphoneMessage> GetUnreadMessages()
    {
        return messages.FindAll(m => !m.isRead);
    }

    // Ottiene un messaggio specifico per ID.
    public SmartphoneMessage GetMessage(string messageId)
    {
        return messages.Find(m => m.id == messageId);
    }


    // Riproduce un suono se disponibile.
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }


    // Riproduce il suono di click per i bottoni UI.
    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }

 
    // Verifica se ci sono messaggi non letti.
    public bool HasUnreadMessages()
    {
        return UnreadCount > 0;
    }

    
    // Rimuove tutti i messaggi (per reset/debug).
    public void ClearAllMessages()
    {
        messages.Clear();
        Debug.Log("[SmartphoneManager] Tutti i messaggi rimossi");
    }
}
